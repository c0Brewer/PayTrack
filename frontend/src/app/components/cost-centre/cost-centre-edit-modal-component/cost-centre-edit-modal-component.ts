import { Component, EventEmitter, Input, OnChanges, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { CostCentreDto } from '../../../types/exporter';
import { ModalComponent } from '../../general/modal-component/modal-component';

@Component({
  selector: 'app-cost-centre-edit-modal-component',
  imports: [FormsModule, ModalComponent],
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
  };

  @Output() saveEvent = new EventEmitter<CostCentreDto>();
  @Output() closeEvent = new EventEmitter<void>();

  originalCostCentre: CostCentreDto | null = null;

  ngOnChanges(): void {
    if (this.costCentre) {
      this.originalCostCentre = structuredClone(this.costCentre);
    }
  }

  get isCreating(): boolean {
    return this.costCentre.id === -1;
  }

  hasChanged(): boolean {
    if (!this.originalCostCentre) return false;
    return (
      this.costCentre.name !== this.originalCostCentre.name ||
      this.costCentre.description !== this.originalCostCentre.description ||
      this.costCentre.displayColor !== this.originalCostCentre.displayColor
    );
  }

  onSave(): void {
    if (!this.isCreating && !this.hasChanged()) {
      this.onClose();
      return;
    }
    this.saveEvent.emit(this.costCentre);
  }

  onClose(): void {
    this.closeEvent.emit();
  }
}
