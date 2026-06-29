import { ChangeDetectorRef, Component, OnInit } from '@angular/core';

import { CostCentreService } from '../../../services/cost-centre/cost-centre-service';
import { NotificationService } from '../../../services/notification/notification-service';
import { SeasonService } from '../../../services/season/season-service';
import { TeamService } from '../../../services/team/team-service';
import {
  CostCentreDto,
  GetCostCentreOptions,
  GetTeamOptions,
  SeasonDto,
  TeamDto,
} from '../../../types/exporter';
import { StatBoxComponent } from '../../general/boxes/stat-box-component/stat-box-component';
import { PaginationComponent } from '../../general/pagination-component/pagination-component';
import { CostCentreDeleteComponent } from '../cost-centre-delete-component/cost-centre-delete-component';
import { CostCentreEditModalComponent } from '../cost-centre-edit-modal-component/cost-centre-edit-modal-component';
import { CostCentreFilterComponent } from '../cost-centre-filter-component/cost-centre-filter-component';
import { CostCentreListComponent } from '../cost-centre-list-component/cost-centre-list-component';

@Component({
  selector: 'app-cost-centre-management-component',
  imports: [
    CostCentreListComponent,
    CostCentreFilterComponent,
    CostCentreEditModalComponent,
    CostCentreDeleteComponent,
    PaginationComponent,
    StatBoxComponent,
  ],
  templateUrl: './cost-centre-management-component.html',
  styleUrl: './cost-centre-management-component.scss',
})
export class CostCentreManagementComponent implements OnInit {
  private static readonly CurrencyFormatter = new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'EUR',
    minimumFractionDigits: 0,
    maximumFractionDigits: 0,
  });

  constructor(
    private readonly costCentreService: CostCentreService,
    private readonly seasonService: SeasonService,
    private readonly teamService: TeamService,
    private readonly cdr: ChangeDetectorRef,
    private readonly notificationService: NotificationService,
  ) {}

  costCentres: CostCentreDto[] = [];
  teams: TeamDto[] = [];
  seasons: SeasonDto[] = [];
  editingCostCentre: CostCentreDto | null = null;
  deletingCostCentre: CostCentreDto | null = null;

  limitSelection: number[] = [10, 25, 50];

  limit: number = this.limitSelection[0];
  page: number = 0;
  totalCount: number = 0;
  hasNext: boolean = false;
  hasPrev: boolean = false;
  activeCostCentreCount: number = 0;
  costCentresWithBudgetsCount: number = 0;
  totalBudgetAmount: number = 0;

  filterOptions: GetCostCentreOptions = {
    Name: undefined,
    Description: undefined,
    MinBudget: undefined,
    MaxBudget: undefined,
    Limit: this.limit,
    Offset: this.page * this.limit,
  };

  ngOnInit(): void {
    this.load();
    this.loadTeams();
    this.loadSeasons();
  }

  loadTeams(): void {
    const queryOptions: GetTeamOptions = {
      IncludeMembers: false,
      IncludeBudgets: false,
      Limit: 1000,
      Offset: 0,
    };

    this.teamService.getTeams(queryOptions).subscribe({
      next: (data) => {
        this.teams = data.items ?? [];
        this.cdr.markForCheck();
      },
      error: (err: Error) => {
        this.notificationService.showError('Could not load teams: ' + err.message);
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

  load(): void {
    const queryOptions: GetCostCentreOptions = {
      Name: this.filterOptions?.Name ?? undefined,
      Description: this.filterOptions?.Description ?? undefined,
      MinBudget: this.filterOptions?.MinBudget ?? undefined,
      MaxBudget: this.filterOptions?.MaxBudget ?? undefined,
      Limit: this.limit,
      Offset: this.page * this.limit,
    };

    const summaryQueryOptions: GetCostCentreOptions = {
      Name: queryOptions.Name,
      Description: queryOptions.Description,
      MinBudget: queryOptions.MinBudget,
      MaxBudget: queryOptions.MaxBudget,
    };

    this.costCentreService.getCostCentres(queryOptions).subscribe({
      next: (data) => {
        if (data?.items) {
          this.costCentres = data.items;
          this.totalCount = data.totalCount;
          this.hasNext = data.hasNext ?? false;
          this.hasPrev = data.hasPrevious ?? false;
          this.cdr.markForCheck();
        } else {
          this.notificationService.showError('Error while loading items');
        }
      },
      error: (err: Error) => {
        this.notificationService.showError('Could not load cost centres: ' + err.message);
      },
    });

    this.costCentreService.getCostCentres(summaryQueryOptions).subscribe({
      next: (data) => {
        const summaryItems = data.items ?? [];
        this.activeCostCentreCount = summaryItems.filter(
          (costCentre) => costCentre.isActive,
        ).length;
        this.costCentresWithBudgetsCount = summaryItems.filter(
          (costCentre) => (costCentre.budgets?.length ?? 0) > 0,
        ).length;
        this.totalBudgetAmount = summaryItems.reduce(
          (sum, costCentre) =>
            sum +
            (costCentre.budgets ?? []).reduce(
              (budgetSum, budget) => budgetSum + (budget.targetAmount ?? 0),
              0,
            ),
          0,
        );
        this.cdr.markForCheck();
      },
      error: (err: Error) => {
        this.notificationService.showError('Could not load cost centre summary: ' + err.message);
      },
    });
  }

  get summaryBannerText(): string {
    if (this.totalCount === 0) {
      return 'No cost centres match the current filters.';
    }

    const filterOptions = this.filterOptions;
    const filterActive = Boolean(
      filterOptions?.Name ||
      filterOptions?.Description ||
      filterOptions?.MinBudget != null ||
      filterOptions?.MaxBudget != null,
    );

    return filterActive
      ? `Review ${this.totalCount} matching cost centres, their budgets, and current availability.`
      : `Organize ${this.totalCount} cost centres, budgets, and their current availability.`;
  }

  get totalBudgetDisplay(): string {
    return CostCentreManagementComponent.CurrencyFormatter.format(this.totalBudgetAmount);
  }

  updateFilterOptions(options: GetCostCentreOptions): void {
    if (this.filterOptions && options) {
      this.filterOptions.Name = options.Name;
      this.filterOptions.Description = options.Description;
      this.filterOptions.MinBudget = options.MinBudget;
      this.filterOptions.MaxBudget = options.MaxBudget;
      this.page = 0;
      this.load();
    }
  }

  openCreate(): void {
    this.editingCostCentre = {
      id: -1,
      name: '',
      description: null,
      displayColor: null,
      budgets: [],
      isActive: true,
    };
  }

  openEdit(costCentre: CostCentreDto): void {
    if (this.hasMissingBudgetSeasons(costCentre)) {
      this.seasonService.getSeasons({ IncludeInactive: true }).subscribe({
        next: (seasons) => {
          this.seasons = seasons;
          this.editingCostCentre = costCentre;
          this.cdr.markForCheck();
        },
        error: (err: Error) => {
          this.notificationService.showError('Could not load seasons: ' + err.message);
          this.editingCostCentre = costCentre;
          this.cdr.markForCheck();
        },
      });
      return;
    }

    this.editingCostCentre = costCentre;
  }

  closeEdit(): void {
    this.editingCostCentre = null;
  }

  onCostCentreSaved(): void {
    this.load();
    this.closeEdit();
  }

  openDelete(costCentre: CostCentreDto): void {
    this.deletingCostCentre = costCentre;
  }

  closeDelete(): void {
    this.deletingCostCentre = null;
  }

  onCostCentreDeleted(): void {
    this.load();
    this.closeDelete();
  }

  getTotalPages(): number {
    const pageNumber = Math.ceil(this.totalCount / this.limit);
    return pageNumber > 0 ? pageNumber : 1;
  }

  onLimitChange(limit: number): void {
    this.limit = limit;
    this.page = 0;
    this.load();
  }

  nextPage(): void {
    this.page++;
    this.load();
  }

  previousPage(): void {
    if (this.page > 0) {
      this.page--;
      this.load();
    }
  }

  private hasMissingBudgetSeasons(costCentre: CostCentreDto): boolean {
    const knownSeasonIds = new Set(this.seasons.map((season) => season.id));
    return (costCentre.budgets ?? []).some((budget) => !knownSeasonIds.has(budget.seasonId));
  }
}
