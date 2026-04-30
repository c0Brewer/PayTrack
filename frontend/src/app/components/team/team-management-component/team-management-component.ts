import { CommonModule } from '@angular/common';
import { Component, ChangeDetectorRef } from '@angular/core';

import { NotificationService } from '../../../services/notification/notification-service';
import { TeamService } from '../../../services/team/team-service';
import { TeamDto, GetTeamOptions, UpdateTeamDto } from '../../../types/exporter';
import { PaginationComponent } from '../../general/pagination-component/pagination-component';
import { TeamEditModalComponent } from '../team-edit-modal-component/team-edit-modal-component';
import { TeamFilterComponent } from '../team-filter-component/team-filter-component';
import { TeamListComponent } from '../team-list-component/team-list-component';

@Component({
  selector: 'app-team-management-component',
  imports: [
    CommonModule,
    PaginationComponent,
    TeamFilterComponent,
    TeamListComponent,
    TeamEditModalComponent,
  ],
  templateUrl: './team-management-component.html',
  styleUrl: './team-management-component.scss',
})
export class TeamManagementComponent {
  constructor(
    private readonly teamService: TeamService,
    private readonly cdr: ChangeDetectorRef,
    private readonly notificationService: NotificationService,
  ) {}

  teams: TeamDto[] = [];
  editingTeam: TeamDto | null = null;

  limitSelection: number[] = [10, 25, 50];

  limit: number = this.limitSelection[0];
  page: number = 0;
  totalCount: number = 0;
  hasNext: boolean = false;
  hasPrev: boolean = false;

  filterOptions: NonNullable<GetTeamOptions> = {
    Name: undefined,
    Description: undefined,
    MinBudget: undefined,
    MaxBudget: undefined,
    IncludeMembers: true,
    IncludeBudgets: true,
    Limit: this.limit,
    Offset: this.page * this.limit,
  };

  ngOnInit(): void {
    this.loadTeams();
  }

  loadTeams(): void {
    const queryOptions: NonNullable<GetTeamOptions> = {
      Name: this.filterOptions?.Name ?? undefined,
      Description: this.filterOptions?.Description ?? undefined,
      MinBudget: this.filterOptions?.MinBudget ?? undefined,
      MaxBudget: this.filterOptions?.MaxBudget ?? undefined,
      IncludeMembers: true,
      IncludeBudgets: true,
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

  openEditTeam(team: TeamDto): void {
    this.editingTeam = { ...team };
  }

  closeEdit(): void {
    this.editingTeam = null;
  }

  saveTeam(team: TeamDto): void {
    if (!team) return;

    const updateRequest: UpdateTeamDto = {
      name: team.name,
      description: team.description,
      displayColor: team.displayColor,
    };

    this.teamService.updateTeam(team.id, updateRequest).subscribe({
      next: () => {
        this.notificationService.showSuccess('Successfully updated team ' + team.name);
        this.loadTeams();
        this.closeEdit();
      },
      error: (error: Error) => {
        this.notificationService.showError('Could not update Team: ' + error);
      },
    });
  }
}
