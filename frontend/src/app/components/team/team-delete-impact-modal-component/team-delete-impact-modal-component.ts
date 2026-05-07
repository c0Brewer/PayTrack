import { Component, EventEmitter, Input, Output } from '@angular/core';

import { DeleteTeamImpactDto } from '../../../types/exporter';
import { ModalComponent } from '../../general/modal-component/modal-component';

@Component({
  selector: 'app-team-delete-impact-modal-component',
  imports: [ModalComponent],
  templateUrl: './team-delete-impact-modal-component.html',
  styleUrl: './team-delete-impact-modal-component.scss',
})
export class TeamDeleteImpactModalComponent {
  @Input() impact: DeleteTeamImpactDto = {
    teamId: 0,
    teamName: '',
    canDelete: false,
    affectedUserCount: 0,
    blockingBudgetCount: 0,
    blockingTransactionCount: 0,
    invoiceCount: 0,
    warningMessage: '',
  };
  @Input() isTeamActive: boolean = true;

  @Output() confirmDelete = new EventEmitter<void>();
  @Output() closeEvent = new EventEmitter<void>();

  get hasImpact(): boolean {
    return this.impact.canDelete === false;
  }

  get isReadOnlyImpact(): boolean {
    return this.hasImpact && !this.isTeamActive;
  }

  onConfirmDelete(): void {
    this.confirmDelete.emit();
  }

  onClose(): void {
    this.closeEvent.emit();
  }
}
