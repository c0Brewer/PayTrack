import { Component, EventEmitter, Input, Output } from '@angular/core';

import { DeleteCostCentrePreviewDto } from '../../../types/exporter';
import { ModalComponent } from '../../general/modal-component/modal-component';

@Component({
  selector: 'app-cost-centre-delete-preview-modal-component',
  imports: [ModalComponent],
  templateUrl: './cost-centre-delete-preview-modal-component.html',
  styleUrl: './cost-centre-delete-preview-modal-component.scss',
})
export class CostCentreDeletePreviewModalComponent {
  @Input() preview: DeleteCostCentrePreviewDto = {
    costCentreName: '',
    budgetCount: 0,
    transactionCount: 0,
    affectedTeamNames: [],
  };

  @Output() confirmDelete = new EventEmitter<void>();
  @Output() closeEvent = new EventEmitter<void>();

  get hasLinkedRecords(): boolean {
    return this.preview.budgetCount > 0 || this.preview.transactionCount > 0;
  }

  onConfirmDelete(): void {
    this.confirmDelete.emit();
  }

  onClose(): void {
    this.closeEvent.emit();
  }
}
