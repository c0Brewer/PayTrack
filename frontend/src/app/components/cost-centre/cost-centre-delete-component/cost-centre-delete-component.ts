import { Component, EventEmitter, Input, OnChanges, Output } from '@angular/core';

import { CostCentreService } from '../../../services/cost-centre/cost-centre-service';
import { NotificationService } from '../../../services/notification/notification-service';
import { CostCentreDto, DeleteCostCentrePreviewDto } from '../../../types/exporter';
import { ModalComponent } from '../../general/modal-component/modal-component';

@Component({
  selector: 'app-cost-centre-delete-component',
  imports: [ModalComponent],
  templateUrl: './cost-centre-delete-component.html',
  styleUrl: './cost-centre-delete-component.scss',
})
export class CostCentreDeleteComponent implements OnChanges {
  constructor(
    private readonly costCentreService: CostCentreService,
    private readonly notificationService: NotificationService,
  ) {}

  @Input() costCentre: CostCentreDto | null = null;

  @Output() deleteEvent = new EventEmitter<void>();
  @Output() closeEvent = new EventEmitter<void>();

  deletePreview: DeleteCostCentrePreviewDto | null = null;

  ngOnChanges(): void {
    this.deletePreview = null;

    if (!this.costCentre) {
      return;
    }

    this.costCentreService.getDeletePreview(this.costCentre.id).subscribe({
      next: (preview) => {
        this.deletePreview = preview;
      },
      error: (err: Error) => {
        this.notificationService.showError('Could not load delete preview: ' + err.message);
        this.onClose();
      },
    });
  }

  get hasLinkedDeleteRecords(): boolean {
    return (
      (this.deletePreview?.budgetCount ?? 0) > 0 || (this.deletePreview?.transactionCount ?? 0) > 0
    );
  }

  onClose(): void {
    this.closeEvent.emit();
  }

  confirmDelete(): void {
    if (!this.costCentre) return;

    this.costCentreService.deleteCostCentre(this.costCentre.id).subscribe({
      next: (result) => {
        if (result) {
          this.notificationService.showSuccess(
            `Cost centre "${this.costCentre!.name}" deactivated`,
          );
        } else {
          this.notificationService.showSuccess(
            `Cost centre "${this.costCentre!.name}" deleted successfully`,
          );
        }
        this.deleteEvent.emit();
      },
      error: (err: Error) => {
        this.notificationService.showError('Could not delete cost centre: ' + err.message);
      },
    });
  }
}
