import { DatePipe } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { BudgetDto, CostCentreDto, UpsertBudgetEntryDto } from '../../../types/exporter';
import { CostCentreSaveEvent } from '../../../types/misc-types';
import { ModalComponent } from '../../general/modal-component/modal-component';

interface WorkingBudget {
  originalId: number;
  teamId: number;
  targetAmount: number;
  periodStart: string;
  periodEnd: string;
  markedForDeletion: boolean;
}

type BudgetField = 'teamId' | 'targetAmount' | 'periodStart' | 'periodEnd';

@Component({
  selector: 'app-cost-centre-edit-modal-component',
  imports: [DatePipe, FormsModule, ModalComponent],
  templateUrl: './cost-centre-edit-modal-component.html',
  styleUrl: './cost-centre-edit-modal-component.scss',
})
export class CostCentreEditModalComponent implements OnChanges {
  @Input() costCentre: CostCentreDto = {
    id: -1,
    name: '',
    description: null,
    displayColor: null,
    budgets: [],
    isActive: true,
  };

  @Output() saveEvent = new EventEmitter<CostCentreSaveEvent>();
  @Output() closeEvent = new EventEmitter<void>();

  originalCostCentre: CostCentreDto | null = null;
  workingBudgets: WorkingBudget[] = [];
  newBudgets: UpsertBudgetEntryDto[] = [];
  newBudgetDraft: UpsertBudgetEntryDto = this.emptyDraft();
  touchedBudgetFields: Record<BudgetField, boolean> = this.emptyBudgetTouchedFields();

  ngOnChanges(): void {
    if (this.costCentre) {
      this.originalCostCentre = structuredClone(this.costCentre);
      this.workingBudgets = (this.costCentre.budgets ?? []).map((b: BudgetDto) => ({
        originalId: b.id,
        teamId: b.teamId,
        targetAmount: b.targetAmount,
        periodStart: b.periodStart,
        periodEnd: b.periodEnd,
        markedForDeletion: false,
      }));
      this.newBudgets = [];
      this.newBudgetDraft = this.emptyDraft();
      this.touchedBudgetFields = this.emptyBudgetTouchedFields();
    }
  }

  get isCreating(): boolean {
    return this.costCentre.id === -1;
  }

  hasChanged(): boolean {
    if (!this.originalCostCentre) return false;
    const fieldsChanged =
      this.costCentre.name !== this.originalCostCentre.name ||
      this.costCentre.description !== this.originalCostCentre.description ||
      this.costCentre.displayColor !== this.originalCostCentre.displayColor;
    const budgetsChanged =
      this.newBudgets.length > 0 || this.workingBudgets.some((b) => b.markedForDeletion);
    return fieldsChanged || budgetsChanged;
  }

  toggleBudgetDeletion(budget: WorkingBudget): void {
    budget.markedForDeletion = !budget.markedForDeletion;
  }

  addNewBudget(): void {
    this.markAllBudgetFieldsTouched();

    if (this.isBudgetDraftInvalid()) {
      return;
    }

    this.newBudgets.push({ ...this.newBudgetDraft });
    this.newBudgetDraft = this.emptyDraft();
    this.touchedBudgetFields = this.emptyBudgetTouchedFields();
  }

  removeNewBudget(index: number): void {
    this.newBudgets.splice(index, 1);
  }

  onSave(): void {
    if (!this.isCreating && !this.hasChanged()) {
      this.onClose();
      return;
    }

    const budgetIdsToDelete = this.workingBudgets
      .filter((b) => b.markedForDeletion)
      .map((b) => b.originalId);

    const budgetsToUpsert: UpsertBudgetEntryDto[] = this.newBudgets.map((b) => ({
      ...b,
      id: null,
    }));

    this.saveEvent.emit({ costCentre: this.costCentre, budgetsToUpsert, budgetIdsToDelete });
  }

  onClose(): void {
    this.closeEvent.emit();
  }

  onBudgetFieldBlur(field: BudgetField): void {
    this.touchedBudgetFields[field] = true;
  }

  hasBudgetFieldError(field: BudgetField): boolean {
    return this.touchedBudgetFields[field] && this.getBudgetFieldError(field).length > 0;
  }

  getBudgetFieldError(field: BudgetField): string {
    switch (field) {
      case 'teamId':
        return this.newBudgetDraft.teamId ? '' : 'Team ID is required.';
      case 'targetAmount':
        if (
          this.newBudgetDraft.targetAmount === null ||
          this.newBudgetDraft.targetAmount === undefined ||
          Number.isNaN(Number(this.newBudgetDraft.targetAmount))
        ) {
          return 'Amount is required.';
        }

        if (Number(this.newBudgetDraft.targetAmount) < 0) {
          return 'Amount must be non-negative.';
        }

        return '';
      case 'periodStart':
        return this.newBudgetDraft.periodStart ? '' : 'Period start is required.';
      case 'periodEnd':
        if (!this.newBudgetDraft.periodEnd) {
          return 'Period end is required.';
        }

        if (
          this.newBudgetDraft.periodStart &&
          this.newBudgetDraft.periodEnd < this.newBudgetDraft.periodStart
        ) {
          return 'Period end must not be before period start.';
        }

        return '';
    }
  }

  private markAllBudgetFieldsTouched(): void {
    this.touchedBudgetFields.teamId = true;
    this.touchedBudgetFields.targetAmount = true;
    this.touchedBudgetFields.periodStart = true;
    this.touchedBudgetFields.periodEnd = true;
  }

  private isBudgetDraftInvalid(): boolean {
    return (
      this.getBudgetFieldError('teamId').length > 0 ||
      this.getBudgetFieldError('targetAmount').length > 0 ||
      this.getBudgetFieldError('periodStart').length > 0 ||
      this.getBudgetFieldError('periodEnd').length > 0
    );
  }

  private emptyBudgetTouchedFields(): Record<BudgetField, boolean> {
    return {
      teamId: false,
      targetAmount: false,
      periodStart: false,
      periodEnd: false,
    };
  }

  private emptyDraft(): UpsertBudgetEntryDto {
    return { id: null, teamId: 0, targetAmount: 0, periodStart: '', periodEnd: '' };
  }
}
