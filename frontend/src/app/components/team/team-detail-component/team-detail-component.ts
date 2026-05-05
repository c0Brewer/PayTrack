import { SlicePipe } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';

import { CostCentreService } from '../../../services/cost-centre/cost-centre-service';
import { NotificationService } from '../../../services/notification/notification-service';
import { TeamService } from '../../../services/team/team-service';
import { CostCentreDto, TeamDto } from '../../../types/exporter';

@Component({
  selector: 'app-team-detail-component',
  imports: [RouterLink, SlicePipe],
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

  formatBudgetAmount(amount: number): string {
    return new Intl.NumberFormat('de-DE', { maximumFractionDigits: 2 }).format(amount);
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
