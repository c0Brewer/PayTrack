import {
  Component,
  EventEmitter,
  Input,
  Output,
} from '@angular/core';

import { CostCentreDto } from '../../../types/exporter';

@Component({
  selector: 'app-cost-centre-list-component',
  imports: [],
  templateUrl: './cost-centre-list-component.html',
  styleUrl: './cost-centre-list-component.scss',
})
export class CostCentreListComponent {
  @Input() costCentres: CostCentreDto[] = [];

  @Output() openEdit = new EventEmitter<CostCentreDto>();
  @Output() openDelete = new EventEmitter<CostCentreDto>();

  onOpenEdit(costCentre: CostCentreDto): void {
    this.openEdit.emit(costCentre);
  }

  onOpenDelete(costCentre: CostCentreDto): void {
    this.openDelete.emit(costCentre);
  }
}
