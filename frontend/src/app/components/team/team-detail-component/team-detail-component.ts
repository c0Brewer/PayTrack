import { SlicePipe } from '@angular/common';
import { EuroPipe } from '../../../pipes/euro.pipe';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';

import { CostCentreService } from '../../../services/cost-centre/cost-centre-service';
import { NotificationService } from '../../../services/notification/notification-service';
import { TeamService } from '../../../services/team/team-service';
import { BudgetDto, BudgetType, CostCentreDto, TeamDto } from '../../../types/exporter';
import { DetailComponent } from '../../general/detail-component/detail-component';

@Component({
  selector: 'app-team-detail-component',
  imports: [DetailComponent, EuroPipe, RouterLink, SlicePipe],
  templateUrl: './team-detail-component.html',
  styleUrl: './team-detail-component.scss',
})
export class TeamDetailComponent implements OnInit {
  constructor(
    private readonly teamService: TeamService,
    private readonly costCentreService: CostCentreService,
    private readonly notificationService: NotificationService,
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly cdr: ChangeDetectorRef,
  ) {}

  team: TeamDto | null = null;
  costCentres: CostCentreDto[] = [];

  get expenseBudgets(): BudgetDto[] {
    return (this.team?.budgets ?? []).filter((b) => b.type === BudgetType.Expense);
  }

  get incomeBudgets(): BudgetDto[] {
    return (this.team?.budgets ?? []).filter((b) => b.type === BudgetType.Income);
  }

  ngOnInit(): void {
    this.loadCostCentres();

    this.route.paramMap.subscribe((params) => {
      const id = Number(params.get('id'));

      this.teamService.getTeamById(id, { IncludeMembers: true, IncludeBudgets: true }).subscribe({
        next: (data) => {
          this.team = data;
          this.cdr.detectChanges();
        },
        error: (err: Error) => {
          this.notificationService.showError('Could not load team: ' + err.message);
        },
      });
    });
  }

  goBack(): void {
    this.router.navigate(['/team']);
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

  getCostCentreName(costCentreId: number): string {
    return (
      this.costCentres.find((costCentre) => costCentre.id === costCentreId)?.name ??
      `Cost Centre #${costCentreId}`
    );
  }

  private loadCostCentres(): void {
    this.costCentreService.getCostCentres({ Limit: 1000, Offset: 0 }).subscribe({
      next: (data) => {
        this.costCentres = data.items ?? [];
        this.cdr.detectChanges();
      },
      error: (err: Error) => {
        this.notificationService.showError('Could not load cost centres: ' + err.message);
      },
    });
  }
}
