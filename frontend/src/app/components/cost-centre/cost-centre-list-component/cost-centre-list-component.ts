import { EuroPipe } from '../../../pipes/euro.pipe';
import {
  AfterViewChecked,
  Component,
  ElementRef,
  EventEmitter,
  Input,
  OnDestroy,
  Output,
} from '@angular/core';
import { RouterLink } from '@angular/router';
import { Tooltip } from 'bootstrap';

import { BudgetDto, CostCentreDto } from '../../../types/exporter';

@Component({
  selector: 'app-cost-centre-list-component',
  imports: [EuroPipe, RouterLink],
  templateUrl: './cost-centre-list-component.html',
  styleUrl: './cost-centre-list-component.scss',
})
export class CostCentreListComponent implements AfterViewChecked, OnDestroy {
  @Input() costCentres: CostCentreDto[] = [];

  @Output() openEdit = new EventEmitter<CostCentreDto>();
  @Output() openDelete = new EventEmitter<CostCentreDto>();

  private tooltips = new Map<Element, Tooltip>();

  constructor(private elementRef: ElementRef<HTMLElement>) {}

  ngAfterViewChecked(): void {
    const tooltipElements = new Set(
      this.elementRef.nativeElement.querySelectorAll('[data-bs-toggle="tooltip"]'),
    );

    tooltipElements.forEach((element) => {
      if (!this.tooltips.has(element)) {
        this.tooltips.set(element, new Tooltip(element));
      }
    });

    this.tooltips.forEach((tooltip, element) => {
      if (!tooltipElements.has(element)) {
        tooltip.dispose();
        this.tooltips.delete(element);
      }
    });
  }

  ngOnDestroy(): void {
    this.tooltips.forEach((tooltip) => tooltip.dispose());
    this.tooltips.clear();
  }

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
