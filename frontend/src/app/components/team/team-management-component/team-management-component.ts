import { CommonModule } from '@angular/common';
import { Component, ChangeDetectorRef } from '@angular/core';

import { CostCentreService } from '../../../services/cost-centre/cost-centre-service';
import { NotificationService } from '../../../services/notification/notification-service';
import { SeasonService } from '../../../services/season/season-service';
import { TeamService } from '../../../services/team/team-service';
import {
  CostCentreDto,
  CreateTeamBudgetEntryDto,
  CreateTeamRequestDto,
  DeleteTeamImpactDto,
  TeamDto,
  GetTeamOptions,
  UpdateTeamDto,
  UpsertTeamBudgetEntryDto,
  SeasonDto,
} from '../../../types/exporter';
import { TeamSaveEvent } from '../../../types/misc-types';
import { StatBoxComponent } from '../../general/boxes/stat-box-component/stat-box-component';
import { PaginationComponent } from '../../general/pagination-component/pagination-component';
import { TeamDeleteImpactModalComponent } from '../team-delete-impact-modal-component/team-delete-impact-modal-component';
import { TeamEditModalComponent } from '../team-edit-modal-component/team-edit-modal-component';
import { TeamFilterComponent } from '../team-filter-component/team-filter-component';
import { TeamListComponent } from '../team-list-component/team-list-component';

@Component({
  selector: 'app-team-management-component',
  imports: [
    CommonModule,
    StatBoxComponent,
    PaginationComponent,
    TeamFilterComponent,
    TeamListComponent,
    PaginationComponent,
    TeamFilterComponent,
    TeamListComponent,
    TeamEditModalComponent,
    TeamDeleteImpactModalComponent,
  ],
  templateUrl: './team-management-component.html',
  styleUrl: './team-management-component.scss',
})
export class TeamManagementComponent {
  constructor(
    private readonly costCentreService: CostCentreService,
    private readonly seasonService: SeasonService,
    private readonly teamService: TeamService,
    private readonly cdr: ChangeDetectorRef,
    private readonly notificationService: NotificationService,
  ) {}

  teams: TeamDto[] = [];
  activeStatusPendingIds = new Set<number>();
  costCentres: CostCentreDto[] = [];
  seasons: SeasonDto[] = [];
  editingTeam: TeamDto | null = null;
  deletingTeam: TeamDto | null = null;
  deleteImpact: DeleteTeamImpactDto | null = null;

  limitSelection: number[] = [10, 25, 50];

  limit: number = this.limitSelection[0];
  page: number = 0;
  totalCount: number = 0;
  totalTeamCount: number = 0;
  hasNext: boolean = false;
  hasPrev: boolean = false;

  filterOptions: NonNullable<GetTeamOptions> = {
    Name: undefined,
    Description: undefined,
    IsActive: undefined,
    IncludeMembers: true,
    IncludeBudgets: true,
    Limit: this.limit,
    Offset: this.page * this.limit,
  };

  ngOnInit(): void {
    this.loadCostCentres();
    this.loadSeasons();
    this.loadTeams();
    this.loadTeamStats();
  }

  loadTeamStats(): void {
    this.teamService
      .getTeams({
        IncludeMembers: false,
        IncludeBudgets: false,
        Limit: 1,
        Offset: 0,
      })
      .subscribe({
        next: (data) => {
          this.totalTeamCount = data.totalCount ?? 0;
          this.cdr.markForCheck();
        },
        error: (err) => {
          this.notificationService.showError(err);
        },
      });
  }

  loadCostCentres(): void {
    this.costCentreService
      .getCostCentres({
        Limit: 1000,
        Offset: 0,
      })
      .subscribe({
        next: (data) => {
          this.costCentres = data?.items ?? [];
          this.cdr.markForCheck();
        },
        error: (err: Error) => {
          this.notificationService.showError('Could not load cost centres: ' + err.message);
        },
      });
  }

  loadSeasons(): void {
    this.seasonService.getSeasons({ IncludeInactive: true }).subscribe({
      next: (seasons) => {
        this.seasons = seasons;
        this.cdr.markForCheck();
      },
      error: (err: Error) => {
        this.notificationService.showError('Could not load seasons: ' + err.message);
      },
    });
  }

  loadTeams(): void {
    const queryOptions: NonNullable<GetTeamOptions> = {
      Name: this.filterOptions?.Name ?? undefined,
      Description: this.filterOptions?.Description ?? undefined,
      IsActive: this.filterOptions?.IsActive ?? undefined,
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
      this.filterOptions.IsActive = options.IsActive;
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

  get total(): number {
    return this.teams.reduce(
      (sum, team) =>
        sum +
        (team.budgets?.reduce((budgetSum, budget) => budgetSum + (budget.targetAmount ?? 0), 0) ??
          0),
      0,
    );
  }

  openCreate(): void {
    this.editingTeam = {
      id: -1,
      name: '',
      description: '',
      displayColor: this.getDefaultDisplayColor(),
      isActive: true,
      members: [],
      budgets: [],
    };
  }

  openEditTeam(team: TeamDto): void {
    this.editingTeam = structuredClone(team);
  }

  closeEdit(): void {
    this.editingTeam = null;
  }

  openDeleteTeam(team: TeamDto): void {
    this.teamService.getDeleteImpact(team.id).subscribe({
      next: (impact) => {
        this.deletingTeam = team;
        this.deleteImpact = impact;
        this.closeEdit();
        this.cdr.markForCheck();
      },
      error: (err: Error) => {
        this.notificationService.showError('Could not load delete impact: ' + err.message);
      },
    });
  }

  closeDelete(): void {
    this.deletingTeam = null;
    this.deleteImpact = null;
  }

  confirmDelete(): void {
    if (!this.deletingTeam) return;

    this.teamService.deleteTeam(this.deletingTeam.id).subscribe({
      next: (result) => {
        if (result) {
          this.notificationService.showSuccess(`Team "${this.deletingTeam!.name}" deactivated`);
        } else {
          this.notificationService.showSuccess(
            `Team "${this.deletingTeam!.name}" deleted successfully`,
          );
        }
        this.closeDelete();
        this.loadTeams();
      },
      error: (err: Error) => {
        this.notificationService.showError('Could not delete team: ' + err.message);
      },
    });
  }

  saveTeam(event: TeamSaveEvent): void {
    const { team, budgetsToUpsert, budgetIdsToDelete } = event;

    if (team.id === -1) {
      const createRequest: CreateTeamRequestDto = {
        name: team.name,
        description: team.description,
        displayColor: team.displayColor,
        budgets:
          budgetsToUpsert.length > 0
            ? budgetsToUpsert.map(
                ({
                  name,
                  description,
                  costCentreId,
                  seasonId,
                  targetAmount,
                  periodStart,
                  periodEnd,
                }): CreateTeamBudgetEntryDto => ({
                  name,
                  description,
                  costCentreId,
                  seasonId,
                  targetAmount,
                  periodStart: this.toApiDateTime(periodStart),
                  periodEnd: this.toApiDateTime(periodEnd),
                }),
              )
            : undefined,
      };

      this.teamService.createTeam(createRequest).subscribe({
        next: () => {
          this.notificationService.showSuccess('Successfully created team ' + team.name);
          this.loadTeams();
          this.closeEdit();
        },
        error: (error: Error) => {
          this.notificationService.showError('Could not create Team: ' + error);
        },
      });

      return;
    }

    const updateRequest: UpdateTeamDto = {
      name: team.name,
      description: team.description,
      displayColor: team.displayColor,
      budgetsToUpsert:
        budgetsToUpsert.length > 0
          ? budgetsToUpsert.map((budget) => this.normalizeBudgetDates(budget))
          : undefined,
      budgetIdsToDelete: budgetIdsToDelete.length > 0 ? budgetIdsToDelete : undefined,
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

  private getDefaultDisplayColor(): string {
    return '#2563eb';
  }

  private normalizeBudgetDates(budget: UpsertTeamBudgetEntryDto): UpsertTeamBudgetEntryDto {
    return {
      ...budget,
      periodStart: this.toApiDateTime(budget.periodStart),
      periodEnd: this.toApiDateTime(budget.periodEnd),
    };
  }

  private toApiDateTime(value: string): string {
    if (/^\d{4}-\d{2}-\d{2}$/.test(value)) {
      return `${value}T00:00:00.000Z`;
    }

    return value;
  }

  private setActiveStatusPending(teamId: number, isPending: boolean): void {
    const nextPendingIds = new Set(this.activeStatusPendingIds);

    if (isPending) {
      nextPendingIds.add(teamId);
    } else {
      nextPendingIds.delete(teamId);
    }

    this.activeStatusPendingIds = nextPendingIds;
  }

  toggleActive(team: TeamDto): void {
    if (this.activeStatusPendingIds.has(team.id)) {
      return;
    }

    const nextIsActive = !team.isActive;
    const updateRequest: UpdateTeamDto = {
      isActive: nextIsActive,
    };

    this.setActiveStatusPending(team.id, true);

    this.teamService.updateTeam(team.id, updateRequest).subscribe({
      next: () => {
        this.notificationService.showSuccess(
          'Successfully changed active status of user ' + team.name,
        );
        this.teams = this.teams.map((currentTeam) =>
          currentTeam.id === team.id ? { ...currentTeam, isActive: nextIsActive } : currentTeam,
        );
        this.setActiveStatusPending(team.id, false);
        this.cdr.markForCheck();
      },
      error: (error: Error) => {
        this.setActiveStatusPending(team.id, false);
        this.notificationService.showError('Could not update User: ' + error.message);
        this.cdr.markForCheck();
      },
    });
  }
}
