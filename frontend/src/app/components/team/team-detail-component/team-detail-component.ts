import { SlicePipe } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { NotificationService } from '../../../services/notification/notification-service';
import { TeamService } from '../../../services/team/team-service';
import { TeamDto } from '../../../types/exporter';

@Component({
  selector: 'app-team-detail-component',
  imports: [SlicePipe],
  templateUrl: './team-detail-component.html',
  styleUrl: './team-detail-component.scss',
})
export class TeamDetailComponent implements OnInit {
  constructor(
    private readonly teamService: TeamService,
    private readonly notificationService: NotificationService,
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly cdr: ChangeDetectorRef,
  ) {}

  team: TeamDto | null = null;

  ngOnInit(): void {
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
}
