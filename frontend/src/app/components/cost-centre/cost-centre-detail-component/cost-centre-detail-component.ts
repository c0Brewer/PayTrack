import { SlicePipe } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { EuroPipe } from '../../../pipes/euro.pipe';
import { CostCentreService } from '../../../services/cost-centre/cost-centre-service';
import { NotificationService } from '../../../services/notification/notification-service';
import { BudgetDto, BudgetType, CostCentreDto } from '../../../types/exporter';
import { BoxComponent } from '../../general/boxes/box-component/box-component';
import { DetailComponent } from '../../general/detail-component/detail-component';

@Component({
  selector: 'app-cost-centre-detail-component',
  imports: [BoxComponent, DetailComponent, EuroPipe, SlicePipe],
  templateUrl: './cost-centre-detail-component.html',
  styleUrl: './cost-centre-detail-component.scss',
})
export class CostCentreDetailComponent implements OnInit {
  constructor(
    private readonly costCentreService: CostCentreService,
    private readonly notificationService: NotificationService,
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly cdr: ChangeDetectorRef,
  ) {}

  costCentre: CostCentreDto | null = null;

  get expenseBudgets(): BudgetDto[] {
    return (this.costCentre?.budgets ?? []).filter((b) => b.type === BudgetType.Expense);
  }

  get incomeBudgets(): BudgetDto[] {
    return (this.costCentre?.budgets ?? []).filter((b) => b.type === BudgetType.Income);
  }

  ngOnInit(): void {
    this.route.paramMap.subscribe((params) => {
      const id = Number(params.get('id'));

      this.costCentreService.getCostCentre(id).subscribe({
        next: (data) => {
          this.costCentre = data;
          this.cdr.detectChanges();
        },
        error: (err: Error) => {
          this.notificationService.showError('Could not load cost centre: ' + err.message);
        },
      });
    });
  }

  goBack(): void {
    this.router.navigate(['/cost-centre']);
  }

  getPaidPercent(budget: BudgetDto): number {
    if (!budget.targetAmount || budget.targetAmount <= 0) return 0;
    return Math.min((Math.max(0, budget.paidAmount) / budget.targetAmount) * 100, 100);
  }

  getApprovedPercent(budget: BudgetDto): number {
    if (!budget.targetAmount || budget.targetAmount <= 0) return 0;
    const netTotal = Math.max(0, budget.paidAmount + budget.approvedAmount);
    const totalPercent = Math.min((netTotal / budget.targetAmount) * 100, 100);
    return totalPercent - this.getPaidPercent(budget);
  }

  isOverBudget(budget: BudgetDto): boolean {
    return budget.paidAmount + budget.approvedAmount > (budget.targetAmount || 0);
  }
}
