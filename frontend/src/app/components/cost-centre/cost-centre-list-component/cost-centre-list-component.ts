import { CurrencyPipe } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';

import { BudgetDto, CostCentreDto } from '../../../types/exporter';

@Component({
  selector: 'app-cost-centre-list-component',
  imports: [CurrencyPipe],
  templateUrl: './cost-centre-list-component.html',
  styleUrl: './cost-centre-list-component.scss',
})
export class CostCentreListComponent {
  @Input() costCentres: CostCentreDto[] = [];

  @Output() openEdit = new EventEmitter<CostCentreDto>();
  @Output() openDelete = new EventEmitter<CostCentreDto>();

  getActiveBudget(budgets: BudgetDto[] | null | undefined): BudgetDto | undefined {
    if (!budgets) return undefined;
    const now = new Date();
    return budgets.find((b) => new Date(b.periodStart) <= now && now <= new Date(b.periodEnd));
  }

  onOpenEdit(costCentre: CostCentreDto): void {
    this.openEdit.emit(costCentre);
  }

  onOpenDelete(costCentre: CostCentreDto): void {
    this.openDelete.emit(costCentre);
  }
}
