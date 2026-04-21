import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';

import { NotificationService } from '../../../services/notification/notification-service';
import { TeamService } from '../../../services/team/team-service';
import { TeamDto, GetTeamOptions } from '../../../types/exporter';

@Component({
  selector: 'app-team-list-component',
  imports: [CommonModule],
  templateUrl: './team-list-component.html',
  styleUrl: './team-list-component.scss',
})
export class TeamListComponent implements OnInit {
  constructor(
    private readonly teamService: TeamService,
    private readonly cdr: ChangeDetectorRef,
    private readonly notificationService: NotificationService,
  ) {}

  teams: TeamDto[] = [];

  limitSelection: number[] = [10, 25, 50];

  limit: number = this.limitSelection[0];
  page: number = 0;
  totalCount: number = 0;
  hasNext: boolean = false;
  hasPrev: boolean = false;

  filterOptions: GetTeamOptions = {
    Name: undefined,
    Description: undefined,
    MinBudget: undefined,
    MaxBudget: undefined,
    IncludeMembers: undefined,
    IncludeBudgets: undefined,
    Limit: this.limit,
    Offset: this.page * this.limit,
  };

  ngOnInit(): void {
    this.loadTeams();
  }

  loadTeams(): void {
    const queryOptions: GetTeamOptions = {
      Name: this.filterOptions?.Name ?? undefined,
      Description: this.filterOptions?.Description ?? undefined,
      MinBudget: this.filterOptions?.MinBudget ?? undefined,
      MaxBudget: this.filterOptions?.MaxBudget ?? undefined,
      IncludeMembers: this.filterOptions?.IncludeMembers ?? undefined,
      IncludeBudgets: this.filterOptions?.IncludeBudgets ?? undefined,
      Limit: this.limit,
      Offset: this.page * this.limit,
    };

    this.teamService.getTeams(queryOptions).subscribe({
      next: (data) => {
        if (data?.items) {
          this.teams = data.items;
          this.totalCount = data.totalCount;
          this.hasNext = data.hasNext ?? false;
          this.hasPrev = data.hasPrevious ?? false;

          // Mark for refresh
          this.cdr.markForCheck();
        } else {
          this.notificationService.showError('Error while loading Items');
        }
      },
      error: (err) => {
        this.notificationService.showError(err);
      },
    });
  }

  updateFilterOptions(options: GetTeamOptions): void {
    if (this.filterOptions && options) {
      this.filterOptions.Name = options.Name;
      this.filterOptions.Description = options.Description;
      this.filterOptions.MinBudget = options.MinBudget;
      this.filterOptions.MaxBudget = options.MaxBudget;
      this.filterOptions.IncludeMembers = options.IncludeMembers;
      this.filterOptions.IncludeBudgets = options.IncludeBudgets;
      this.page = 0;
      this.loadTeams();
    }
  }

  getTotalPages(): number {
    const pageNumber = Math.ceil(this.totalCount / this.limit);
    return pageNumber > 0 ? pageNumber : 1;
  }

  onLimitChange(limit: number): void {
    this.limit = limit;
    this.page = 0;
    this.loadTeams();
  }

  nextPage(): void {
    this.page++;
    this.loadTeams();
  }

  previousPage(): void {
    if (this.page > 0) {
      this.page--;
      this.loadTeams();
    }
  }
}
