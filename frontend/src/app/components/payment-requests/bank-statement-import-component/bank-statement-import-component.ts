import { Component, computed, input, signal } from '@angular/core';

import { EuroPipe } from '../../../pipes/euro.pipe';
import { BankStatementService } from '../../../services/bank-statement-service/bank-statement-service';
import { NotificationService } from '../../../services/notification/notification-service';
import {
  BankStatementEntryDto,
  BankStatementMatchResultDto,
  BankStatementUpdateRequestDto,
  TransactionStatus,
  TransactionStatusLabels,
} from '../../../types/exporter';
import { StatBoxComponent } from '../../general/boxes/stat-box-component/stat-box-component';
import { ModalComponent } from '../../general/modal-component/modal-component';

type Phase = 'upload' | 'review';

// Raw shape from the bank JSON export — we only extract what the API needs
interface RawBankEntry {
  booking?: string;
  partnerName?: string | null;
  partnerAccount?: { iban?: string; bic?: string } | null;
  amount?: { value?: number; precision?: number; currency?: string } | null;
  receiverReference?: string | null;
  reference?: string | null;
}

@Component({
  selector: 'app-bank-statement-import-component',
  imports: [StatBoxComponent, EuroPipe, ModalComponent],
  templateUrl: './bank-statement-import-component.html',
  styleUrl: './bank-statement-import-component.scss',
})
export class BankStatementImportComponent {
  constructor(
    private readonly bankStatementService: BankStatementService,
    private readonly notificationService: NotificationService,
  ) {}

  showPageHeader = input(true);

  // ── state ──────────────────────────────────────────────────────────────────
  phase = signal<Phase>('upload');
  isDragging = signal(false);
  isLoading = signal(false);

  selectedFileName = signal<string | null>(null);
  parsedEntries = signal<BankStatementEntryDto[]>([]);

  /** Working copy of match results; extended with local state flags */
  results = signal<
    (BankStatementMatchResultDto & { skipped: boolean; expanded: boolean; _entryId: string })[]
  >([]);

  sortMode = signal<'score' | 'original'>('score');

  // ── computed helpers ───────────────────────────────────────────────────────
  matchedCount = computed(() => this.results().filter((r) => r.hasMatch && !r.skipped).length);
  skippedCount = computed(() => this.results().filter((r) => r.skipped).length);
  unmatchedCount = computed(() => this.results().filter((r) => !r.hasMatch && !r.skipped).length);

  allMatchedCount = computed(() => this.results().filter((r) => r.hasMatch).length);

  displayResults = computed(() => {
    const items = this.results();
    if (this.sortMode() === 'original') return items;
    const matched = [...items.filter((r) => r.hasMatch)].sort(
      (a, b) => (b.matchScore ?? 0) - (a.matchScore ?? 0),
    );
    const noMatch = items.filter((r) => !r.hasMatch);
    return [...matched, ...noMatch];
  });

  noMatchBoundaryIndex = computed(() => {
    if (this.sortMode() !== 'score') return -1;
    return this.results().filter((r) => r.hasMatch).length;
  });

  showNonApprovedWarning = signal(false);
  showFinalConfirm = signal(false);

  nonApprovedMatches = computed(() =>
    this.results().filter(
      (r) =>
        r.hasMatch && !r.skipped && r.matchedTransaction?.status !== TransactionStatus.Approved,
    ),
  );

  matchedUpdates = computed(() => this.results().filter((r) => r.hasMatch && !r.skipped));

  // ── phase 1: upload ────────────────────────────────────────────────────────
  onDragOver(event: DragEvent): void {
    event.preventDefault();
    this.isDragging.set(true);
  }

  onDragLeave(): void {
    this.isDragging.set(false);
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.isDragging.set(false);
    const file = event.dataTransfer?.files?.[0];
    if (file) this.handleFile(file);
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (file) this.handleFile(file);
  }

  private handleFile(file: File): void {
    if (!file.name.endsWith('.json')) {
      this.notificationService.showError('Please upload a .json file.');
      return;
    }
    this.selectedFileName.set(file.name);
    const reader = new FileReader();
    reader.onload = (): void => {
      try {
        const raw: RawBankEntry[] = JSON.parse(reader.result as string);
        const entries = this.mapToEntries(raw);
        this.parsedEntries.set(entries);
      } catch {
        this.notificationService.showError(
          'Could not parse the JSON file. Please check the format.',
        );
        this.selectedFileName.set(null);
      }
    };
    reader.readAsText(file);
  }

  private mapToEntries(raw: RawBankEntry[]): BankStatementEntryDto[] {
    return raw.map((r) => ({
      booking: this.normalizeBookingDate(r.booking),
      partnerName: r.partnerName,
      partnerAccount: r.partnerAccount
        ? { iban: r.partnerAccount.iban, bic: r.partnerAccount.bic }
        : undefined,
      amount: r.amount
        ? {
            value:
              r.amount.precision != null && r.amount.value != null
                ? r.amount.value / Math.pow(10, r.amount.precision)
                : r.amount.value,
            currency: r.amount.currency,
          }
        : undefined,
      receiverReference: r.receiverReference,
      reference: r.reference,
    }));
  }

  submitForMatching(): void {
    if (!this.parsedEntries().length) return;
    this.isLoading.set(true);
    this.bankStatementService.getMatches(this.parsedEntries()).subscribe({
      next: (response) => {
        const enriched = (response.results ?? []).map((r, i) => ({
          ...r,
          skipped: false,
          expanded: false,
          _entryId: `entry-${i}`,
        }));
        this.results.set(enriched);
        this.isLoading.set(false);
        this.phase.set('review');
      },
      error: (err: Error) => {
        this.notificationService.showError(err.message);
        this.isLoading.set(false);
      },
    });
  }

  private normalizeBookingDate(booking: string | undefined): string | undefined {
    if (!booking) return undefined;
    // .NET DateTimeOffset requires +HH:MM but the bank export uses +HHMM (no colon).
    // e.g. "2026-05-21T00:00:00.000+0200" → "2026-05-21T00:00:00.000+02:00"
    return booking.replace(/([+-])(\d{2})(\d{2})$/, '$1$2:$3');
  }

  // ── phase 2: review ────────────────────────────────────────────────────────
  toggleSkip(entryId: string): void {
    this.results.update((list) =>
      list.map((r) => (r._entryId === entryId ? { ...r, skipped: !r.skipped } : r)),
    );
  }

  toggleExpand(entryId: string): void {
    this.results.update((list) =>
      list.map((r) => (r._entryId === entryId ? { ...r, expanded: !r.expanded } : r)),
    );
  }

  formatAmount(entry: BankStatementEntryDto | undefined): string {
    if (!entry?.amount) return '—';
    const v = entry.amount.value ?? 0;
    const c = entry.amount.currency ?? '';
    return `${v.toFixed(2)} ${c}`;
  }

  formatDate(dateStr: string | undefined): string {
    if (!dateStr) return '—';
    return new Date(dateStr).toLocaleDateString('de-AT', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
    });
  }

  scoreLabel(score: number | undefined): string {
    if (score == null) return '';
    if (score >= 7) return 'High';
    if (score >= 5) return 'Medium';
    return 'Low';
  }

  scoreColor(score: number | undefined): string {
    if (score == null) return 'confidence-badge confidence-badge--none';
    if (score >= 7) return 'confidence-badge confidence-badge--high';
    if (score >= 5) return 'confidence-badge confidence-badge--medium';
    return 'confidence-badge confidence-badge--low';
  }

  confirmUpdates(): void {
    if (this.nonApprovedMatches().length > 0) {
      this.showNonApprovedWarning.set(true);
      return;
    }
    this.showFinalConfirm.set(true);
  }

  confirmAnyway(): void {
    this.showNonApprovedWarning.set(false);
    this.showFinalConfirm.set(true);
  }

  proceedWithSubmit(): void {
    this.showFinalConfirm.set(false);
    this.submitUpdates();
  }

  private submitUpdates(): void {
    const updates: BankStatementUpdateRequestDto[] = this.results().map((r) => ({
      entryId: r._entryId,
      matchedTransactionId: r.hasMatch ? (r.matchedTransaction?.id as number | undefined) : null,
      skipped: r.skipped,
    }));

    this.isLoading.set(true);
    this.bankStatementService.applyUpdates(updates).subscribe({
      next: (updated) => {
        this.notificationService.showSuccess(
          'Bank statement import successful. Updated transactions: ' + updated.length,
        );
        this.isLoading.set(false);
        this.reset();
      },
      error: (err: Error) => {
        this.notificationService.showError(err.message);
        this.isLoading.set(false);
      },
    });
  }

  reset(): void {
    this.phase.set('upload');
    this.selectedFileName.set(null);
    this.parsedEntries.set([]);
    this.results.set([]);
    this.isLoading.set(false);
  }

  toggleSortMode(): void {
    this.sortMode.update((m) => (m === 'score' ? 'original' : 'score'));
  }

  getStatusLabel(status: TransactionStatus): string {
    return TransactionStatusLabels[status] ?? 'Unknown';
  }

  formatIban(value: string): string {
    return value
      .replaceAll(' ', '')
      .replace(/[^A-Za-z0-9]/g, '')
      .toUpperCase()
      .replace(/(.{4})/g, '$1 ')
      .trim();
  }
}
