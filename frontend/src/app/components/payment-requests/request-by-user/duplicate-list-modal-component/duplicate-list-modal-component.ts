import { DatePipe } from '@angular/common';
import { Component, EventEmitter, Input, Output, inject } from '@angular/core';

import { DisableOfflineActionDirective } from '../../../../directives/disable-offline-action.directive';
import { EuroPipe } from '../../../../pipes/euro.pipe';
import { OfflineService } from '../../../../services/offline/offline-service';
import {
  DuplicatePaymentRequestByUserDto,
  PaymentRequestByUserDto,
} from '../../../../types/exporter';
import { ModalComponent } from '../../../general/modal-component/modal-component';

export type DuplicateInvoiceSummary = {
  id?: number | null;
  invoiceNumber?: string | null;
  amount?: number | null;
  paidAt?: string | null;
  purposeOfPayment?: string | null;
  user?: { name?: string | null } | null;
  team?: { name?: string | null } | null;
};

type DuplicateInvoiceSelection = 'source' | 'matching';

@Component({
  selector: 'app-duplicate-list-modal-component',
  imports: [DatePipe, EuroPipe, ModalComponent, DisableOfflineActionDirective],
  templateUrl: './duplicate-list-modal-component.html',
  styleUrl: './duplicate-list-modal-component.scss',
})
export class DuplicateListModalComponent {
  protected readonly offlineService = inject(OfflineService);

  @Input() visible = false;
  @Input() loading = false;
  @Input() mode: 'review' | 'submit' = 'review';
  @Input() actionInvoiceId: number | null = null;
  @Input() sourceInvoice: DuplicateInvoiceSummary | null = null;
  @Input() duplicates: DuplicatePaymentRequestByUserDto[] = [];

  @Output() closeModal = new EventEmitter<void>();
  @Output() openDetail = new EventEmitter<PaymentRequestByUserDto>();
  @Output() deleteInvoice = new EventEmitter<PaymentRequestByUserDto>();
  @Output() dismissDuplicate = new EventEmitter<DuplicatePaymentRequestByUserDto>();
  @Output() submitRegardless = new EventEmitter<void>();

  selectedInvoiceByDuplicateId: Record<number, DuplicateInvoiceSelection | undefined> = {};
  deleteConfirmationDuplicateId: number | null = null;
  pendingDeleteInvoice: PaymentRequestByUserDto | null = null;

  onClose(): void {
    this.resetInteractionState();
    this.closeModal.emit();
  }

  onOpenSelectedDetail(duplicate: DuplicatePaymentRequestByUserDto): void {
    const invoice = this.getSelectedInvoice(duplicate);

    if (invoice) {
      this.openDetail.emit(invoice);
    }
  }

  onRequestDeleteSelectedInvoice(duplicate: DuplicatePaymentRequestByUserDto): void {
    const invoice = this.getSelectedInvoice(duplicate);

    if (invoice) {
      this.deleteConfirmationDuplicateId = duplicate.paymentRequestByUser.id;
      this.pendingDeleteInvoice = invoice;
    }
  }

  onCancelDeleteConfirmation(): void {
    this.deleteConfirmationDuplicateId = null;
    this.pendingDeleteInvoice = null;
  }

  onConfirmDeleteSelectedInvoice(): void {
    if (this.pendingDeleteInvoice) {
      this.deleteInvoice.emit(this.pendingDeleteInvoice);
      this.onCancelDeleteConfirmation();
    }
  }

  onDismissDuplicate(duplicate: DuplicatePaymentRequestByUserDto): void {
    this.onCancelDeleteConfirmation();
    this.dismissDuplicate.emit(duplicate);
  }

  onSubmitRegardless(): void {
    this.submitRegardless.emit();
  }

  getInvoiceUserName(invoice: DuplicateInvoiceSummary | null): string {
    return invoice?.user?.name ?? 'Unknown user';
  }

  getInvoiceTeamName(invoice: DuplicateInvoiceSummary | null): string {
    return invoice?.team?.name ?? 'Unknown team';
  }

  getDuplicateUserName(duplicate: DuplicatePaymentRequestByUserDto): string {
    return this.getInvoiceUserName(duplicate.paymentRequestByUser);
  }

  getDuplicateTeamName(duplicate: DuplicatePaymentRequestByUserDto): string {
    return this.getInvoiceTeamName(duplicate.paymentRequestByUser);
  }

  getActionableSourceInvoice(): PaymentRequestByUserDto | null {
    return this.sourceInvoice?.id ? (this.sourceInvoice as PaymentRequestByUserDto) : null;
  }

  selectInvoice(
    duplicate: DuplicatePaymentRequestByUserDto,
    selection: DuplicateInvoiceSelection,
  ): void {
    if (selection === 'source' && !this.getActionableSourceInvoice()) {
      return;
    }

    this.selectedInvoiceByDuplicateId[duplicate.paymentRequestByUser.id] = selection;
    this.onCancelDeleteConfirmation();
  }

  isInvoiceSelected(
    duplicate: DuplicatePaymentRequestByUserDto,
    selection: DuplicateInvoiceSelection,
  ): boolean {
    return this.selectedInvoiceByDuplicateId[duplicate.paymentRequestByUser.id] === selection;
  }

  getSelectedInvoice(duplicate: DuplicatePaymentRequestByUserDto): PaymentRequestByUserDto | null {
    const selection = this.selectedInvoiceByDuplicateId[duplicate.paymentRequestByUser.id];

    if (selection === 'source') {
      return this.getActionableSourceInvoice();
    }

    if (selection === 'matching') {
      return duplicate.paymentRequestByUser;
    }

    return null;
  }

  getSelectedInvoiceLabel(duplicate: DuplicatePaymentRequestByUserDto): string {
    const invoice = this.getSelectedInvoice(duplicate);

    return invoice?.invoiceNumber ?? '-';
  }

  isDeleteConfirmationOpen(duplicate: DuplicatePaymentRequestByUserDto): boolean {
    return this.deleteConfirmationDuplicateId === duplicate.paymentRequestByUser.id;
  }

  isDuplicateActionPending(duplicate: DuplicatePaymentRequestByUserDto): boolean {
    const sourceInvoiceId = this.getActionableSourceInvoice()?.id;
    const matchingInvoiceId = duplicate.paymentRequestByUser.id;

    return (
      this.actionInvoiceId !== null &&
      (this.actionInvoiceId === sourceInvoiceId || this.actionInvoiceId === matchingInvoiceId)
    );
  }

  resetInteractionState(): void {
    this.selectedInvoiceByDuplicateId = {};
    this.onCancelDeleteConfirmation();
  }

  getMatchedFieldLabel(field: string): string {
    switch (field) {
      case 'invoiceNumber':
        return 'Same invoice number';
      case 'similarInvoiceNumber':
        return 'Similar invoice number';
      case 'amount':
        return 'Same amount';
      case 'payday':
        return 'Same payday';
      case 'user':
        return 'Same user';
      case 'team':
        return 'Same team';
      default:
        return field;
    }
  }
}
