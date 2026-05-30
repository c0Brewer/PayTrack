import { CommonModule } from '@angular/common';
import {
  ChangeDetectorRef,
  Component,
  EventEmitter,
  Input,
  OnDestroy,
  OnInit,
  Output,
} from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import Papa from 'papaparse';
import { Subject, takeUntil } from 'rxjs';

import { BudgetService } from '../../../../services/budget/budget-service';
import { NotificationService } from '../../../../services/notification/notification-service';
import { PaymentRequestByTeamService } from '../../../../services/payment-request-by-team/payment-request-by-team-service';
import {
  BudgetDto,
  CostCentreDto,
  CreatePaymentRequestByTeamDto,
  TeamDto,
} from '../../../../types/exporter';
import { ModalComponent } from '../../../general/modal-component/modal-component';
import {
  TypeaheadItem,
  TypeaheadSelectComponent,
} from '../../../general/typeahead-select-component/typeahead-select-component';

// TODO: These column names should eventually be configurable by admins in the settings page.
const CSV_COL_NAME = 'Name';
const CSV_COL_SUMME = 'Summe';

type ImportStep = 'configure' | 'preview' | 'results';

interface ParsedRow {
  rawName: string;
  amount: number;
}

interface PreviewRow {
  rawName: string;
  amount: number;
  userId: number | null;
  displayName: string | null;
  isAutoMatched: boolean;
  status: 'pending' | 'success' | 'error';
  errorMessage?: string;
}

function minDateValidator(min: Date): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    if (!control.value) return null;
    const selected = new Date(control.value);
    const minMidnight = new Date(min.getFullYear(), min.getMonth(), min.getDate());
    return selected < minMidnight ? { minDate: { min: min.toISOString().slice(0, 10) } } : null;
  };
}

@Component({
  selector: 'app-csv-bulk-import-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ModalComponent, TypeaheadSelectComponent],
  templateUrl: './csv-bulk-import-modal.html',
  styleUrl: './csv-bulk-import-modal.scss',
})
export class CsvBulkImportModalComponent implements OnInit, OnDestroy {
  @Input({ required: true }) file!: File;
  @Input({ required: true }) teams!: TeamDto[];
  @Input({ required: true }) costCentres!: CostCentreDto[];
  /** Passed from parent — already loaded, no duplicate HTTP call needed. */
  @Input({ required: true }) allUsers!: TypeaheadItem[];
  @Output() closeEvent = new EventEmitter<void>();

  step: ImportStep = 'configure';
  configForm!: FormGroup;

  budgets: BudgetDto[] = [];
  parsedRows: ParsedRow[] = [];
  previewRows: PreviewRow[] = [];
  unmatchedNames: string[] = [];
  showWarningModal = false;
  isSubmitting = false;

  private readonly destroy$ = new Subject<void>();

  constructor(
    private readonly fb: FormBuilder,
    private readonly cdr: ChangeDetectorRef,
    private readonly paymentRequestByTeamService: PaymentRequestByTeamService,
    private readonly budgetService: BudgetService,
    private readonly notificationService: NotificationService,
  ) {}

  ngOnInit(): void {
    this.buildConfigForm();
    this.parseCsvFile();
    this.configForm
      .get('teamId')!
      .valueChanges.pipe(takeUntil(this.destroy$))
      .subscribe((teamId: number | null) => {
        this.budgets = [];
        this.configForm.get('budgetId')!.setValue(null);
        if (teamId != null) {
          this.loadBudgets(teamId);
        }
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private buildConfigForm(): void {
    this.configForm = this.fb.group({
      teamId: [null, Validators.required],
      budgetId: [null, Validators.required],
      purposeOfPayment: ['', [Validators.required, Validators.maxLength(255)]],
      dueDate: ['', [Validators.required, minDateValidator(new Date())]],
    });
  }

  private loadBudgets(teamId: number): void {
    this.budgetService
      .getBudgets({ TeamId: teamId })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result) => {
          const today = new Date();
          this.budgets = (result.items ?? []).filter(
            (b) =>
              b.name.trim() !== '' &&
              new Date(b.periodStart) <= today &&
              new Date(b.periodEnd) >= today,
          );
          this.cdr.detectChanges();
        },
        error: () => this.notificationService.showError('Failed to load budgets.'),
      });
  }

  getCostCentreName(costCentreId: number): string {
    return this.costCentres.find((cc) => cc.id === costCentreId)?.name ?? '';
  }

  private parseCsvFile(): void {
    Papa.parse(this.file, {
      header: false,
      skipEmptyLines: true,
      complete: (result: Papa.ParseResult<string[]>) => {
        const rows = result.data;

        const normalize = (s: string): string => s.trim();

        const headerRowIndex = rows.findIndex(
          (row) =>
            row.some((cell) => normalize(cell) === CSV_COL_NAME) &&
            row.some((cell) => normalize(cell) === CSV_COL_SUMME),
        );

        if (headerRowIndex === -1) {
          this.notificationService.showError(
            `CSV format error: could not find "${CSV_COL_NAME}" and "${CSV_COL_SUMME}" header columns.`,
          );
          this.closeEvent.emit();
          this.cdr.detectChanges();
          return;
        }

        const headers = rows[headerRowIndex];
        const nameIdx = headers.findIndex((cell) => normalize(cell) === CSV_COL_NAME);
        const summeIdx = headers.findIndex((cell) => normalize(cell) === CSV_COL_SUMME);

        this.parsedRows = rows
          .slice(headerRowIndex + 1)
          .filter((row) => (row[nameIdx] ?? '').trim().length > 0)
          .map((row) => ({
            rawName: row[nameIdx].trim(),
            amount: this.parseEuroAmount(row[summeIdx] ?? ''),
          }))
          .filter((row) => row.amount > 0);

        this.cdr.detectChanges();
      },
    });
  }

  parseEuroAmount(raw: string): number {
    const cleaned = raw.replace(/\s/g, '').replace('€', '').replace(/\./g, '').replace(',', '.');
    const val = parseFloat(cleaned);
    return isNaN(val) ? 0 : val;
  }

  private buildPreviewRows(): void {
    const savedAssignments = new Map<string, { userId: number; displayName: string }>();
    for (const row of this.previewRows) {
      if (!row.isAutoMatched && row.userId !== null) {
        savedAssignments.set(row.rawName, { userId: row.userId, displayName: row.displayName! });
      }
    }

    const userMap = new Map<string, TypeaheadItem>();
    for (const user of this.allUsers) {
      const key = user.primaryText.trim().toLowerCase();
      if (userMap.has(key)) {
        // Ambiguous: two users share the same normalized name — sentinel id=-1
        userMap.set(key, { id: -1, primaryText: '__AMBIGUOUS__' });
      } else {
        userMap.set(key, user);
      }
    }

    this.previewRows = this.parsedRows.map((row) => {
      const key = row.rawName.toLowerCase();
      const match = userMap.get(key);
      const isAutoMatched = !!match && match.id !== -1;

      if (!isAutoMatched) {
        const saved = savedAssignments.get(row.rawName);
        if (saved) {
          return {
            rawName: row.rawName,
            amount: row.amount,
            userId: saved.userId,
            displayName: saved.displayName,
            isAutoMatched: false,
            status: 'pending' as const,
          };
        }
      }

      return {
        rawName: row.rawName,
        amount: row.amount,
        userId: isAutoMatched ? (match!.id as number) : null,
        displayName: isAutoMatched ? match!.primaryText : null,
        isAutoMatched,
        status: 'pending' as const,
      };
    });

    this.unmatchedNames = this.previewRows
      .filter((r) => !r.isAutoMatched && r.userId === null)
      .map((r) => r.rawName);
  }

  onNextClicked(): void {
    this.configForm.markAllAsTouched();
    this.buildPreviewRows();

    if (this.unmatchedNames.length > 0) {
      this.showWarningModal = true;
    } else {
      this.step = 'preview';
    }
  }

  onStepClicked(target: 1 | 2): void {
    if (target === 1) {
      this.step = 'configure';
    } else if (this.parsedRows.length > 0) {
      this.onNextClicked();
    }
  }

  onWarningOkClicked(): void {
    this.showWarningModal = false;
    this.step = 'preview';
  }

  onBackClicked(): void {
    this.step = 'configure';
  }

  onUserAssigned(index: number, item: TypeaheadItem): void {
    this.previewRows[index].userId = item.id as number;
    this.previewRows[index].displayName = item.primaryText;
  }

  onUserCleared(index: number): void {
    this.previewRows[index].userId = null;
    this.previewRows[index].displayName = null;
  }

  get allRowsAssigned(): boolean {
    return this.previewRows.every((r) => r.userId !== null);
  }

  get unassignedCount(): number {
    return this.previewRows.filter((r) => r.userId === null).length;
  }

  get canSubmit(): boolean {
    return this.allRowsAssigned && this.configForm.valid && !this.isSubmitting;
  }

  onSubmitAll(): void {
    if (!this.canSubmit) return;
    this.isSubmitting = true;
    this.submitSequentially(0);
  }

  private submitSequentially(index: number): void {
    if (index >= this.previewRows.length) {
      this.isSubmitting = false;
      this.step = 'results';
      this.cdr.detectChanges();
      return;
    }

    const row = this.previewRows[index];
    const payload = this.buildPayload(row);

    this.paymentRequestByTeamService
      .createPaymentRequestByTeam(payload)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          row.status = 'success';
          this.cdr.detectChanges();
          this.submitSequentially(index + 1);
        },
        error: (err: Error) => {
          row.status = 'error';
          row.errorMessage = err.message ?? 'Submission failed.';
          this.cdr.detectChanges();
          this.submitSequentially(index + 1);
        },
      });
  }

  private buildPayload(row: PreviewRow): CreatePaymentRequestByTeamDto {
    const v = this.configForm.value as {
      teamId: string;
      budgetId: string | null;
      purposeOfPayment: string;
      dueDate: string;
    };
    return {
      transaction: {
        teamId: Number(v.teamId),
        amount: row.amount,
        purposeOfPayment: v.purposeOfPayment,
        paidAt: new Date(0).toISOString(),
        budgetId: v.budgetId != null ? Number(v.budgetId) : undefined,
      },
      userToAssignToId: row.userId!,
      dueDate: new Date(v.dueDate).toISOString(),
    };
  }

  onClose(): void {
    this.closeEvent.emit();
  }

  get successCount(): number {
    return this.previewRows.filter((r) => r.status === 'success').length;
  }

  get failureCount(): number {
    return this.previewRows.filter((r) => r.status === 'error').length;
  }

  getError(field: string): string | null {
    const control = this.configForm.get(field);
    if (!control || !control.invalid || !control.touched) return null;
    const errors = control.errors!;
    if (errors['required']) return 'This field is required.';
    if (errors['min']) return `Minimum value is ${errors['min'].min}.`;
    if (errors['maxlength'])
      return `Maximum length is ${errors['maxlength'].requiredLength} characters.`;
    if (errors['minDate']) return 'Due date must be today or in the future.';
    return 'Invalid value.';
  }

  isInvalid(field: string): boolean {
    const control = this.configForm.get(field);
    return !!control && control.invalid && control.touched;
  }
}
