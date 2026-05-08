import { ChangeDetectorRef, Component, OnInit } from '@angular/core';

import { CostCentreService } from '../../../services/cost-centre/cost-centre-service';
import { NotificationService } from '../../../services/notification/notification-service';
import { TeamService } from '../../../services/team/team-service';
import {
  CostCentreDto,
  GetCostCentreOptions,
  CreateCostCentreRequestDto,
  DeleteCostCentrePreviewDto,
  GetTeamOptions,
  TeamDto,
  UpdateCostCentreRequestDto,
  UpsertBudgetEntryDto,
} from '../../../types/exporter';
import { CostCentreSaveEvent } from '../../../types/misc-types';
import { PaginationComponent } from '../../general/pagination-component/pagination-component';
import { CostCentreDeletePreviewModalComponent } from '../cost-centre-delete-preview-modal-component/cost-centre-delete-preview-modal-component';
import { CostCentreEditModalComponent } from '../cost-centre-edit-modal-component/cost-centre-edit-modal-component';
import { CostCentreFilterComponent } from '../cost-centre-filter-component/cost-centre-filter-component';
import { CostCentreListComponent } from '../cost-centre-list-component/cost-centre-list-component';

@Component({
  selector: 'app-cost-centre-management-component',
  imports: [
    CostCentreListComponent,
    CostCentreEditModalComponent,
    CostCentreDeletePreviewModalComponent,
    CostCentreFilterComponent,
    PaginationComponent,
  ],
  templateUrl: './cost-centre-management-component.html',
  styleUrl: './cost-centre-management-component.scss',
})
export class CostCentreManagementComponent implements OnInit {
  constructor(
    private readonly costCentreService: CostCentreService,
    private readonly teamService: TeamService,
    private readonly cdr: ChangeDetectorRef,
    private readonly notificationService: NotificationService,
  ) {}

  costCentres: CostCentreDto[] = [];
  teams: TeamDto[] = [];
  editingCostCentre: CostCentreDto | null = null;
  deletingCostCentre: CostCentreDto | null = null;
  deletePreview: DeleteCostCentrePreviewDto | null = null;

  limitSelection: number[] = [10, 25, 50];

  limit: number = this.limitSelection[0];
  page: number = 0;
  totalCount: number = 0;
  hasNext: boolean = false;
  hasPrev: boolean = false;

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

  load(): void {
    const queryOptions: GetCostCentreOptions = {
      Name: this.filterOptions?.Name ?? undefined,
      Description: this.filterOptions?.Description ?? undefined,
      MinBudget: this.filterOptions?.MinBudget ?? undefined,
      MaxBudget: this.filterOptions?.MaxBudget ?? undefined,
      Limit: this.limit,
      Offset: this.page * this.limit,
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
    this.editingCostCentre = structuredClone(costCentre);
  }

  closeEdit(): void {
    this.editingCostCentre = null;
  }

  save(event: CostCentreSaveEvent): void {
    const { costCentre, budgetsToUpsert, budgetIdsToDelete } = event;
    if (costCentre.id === -1) {
      const request: CreateCostCentreRequestDto = {
        name: costCentre.name,
        description: costCentre.description ?? undefined,
        displayColor: costCentre.displayColor ?? undefined,
        budgets:
          budgetsToUpsert.length > 0
            ? budgetsToUpsert.map(({ name, description, teamId, seasonId, targetAmount, periodStart, periodEnd }) => ({
                name,
                description,
                teamId,
                seasonId,
                targetAmount,
                periodStart: this.toApiDateTime(periodStart),
                periodEnd: this.toApiDateTime(periodEnd),
              }))
            : undefined,
      };
      this.costCentreService.createCostCentre(request).subscribe({
        next: () => {
          this.notificationService.showSuccess('Cost centre created successfully');
          this.load();
          this.closeEdit();
        },
        error: (err: Error) => {
          this.notificationService.showError('Could not create cost centre: ' + err.message);
        },
      });
    } else {
      const request: UpdateCostCentreRequestDto = {
        name: costCentre.name,
        description: costCentre.description ?? undefined,
        displayColor: costCentre.displayColor ?? undefined,
        budgetsToUpsert:
          budgetsToUpsert.length > 0
            ? budgetsToUpsert.map((budget) => this.normalizeBudgetDates(budget))
            : undefined,
        budgetIdsToDelete: budgetIdsToDelete.length > 0 ? budgetIdsToDelete : undefined,
      };
      this.costCentreService.updateCostCentre(costCentre.id, request).subscribe({
        next: () => {
          this.notificationService.showSuccess('Cost centre updated successfully');
          this.load();
          this.closeEdit();
        },
        error: (err: Error) => {
          this.notificationService.showError('Could not update cost centre: ' + err.message);
        },
      });
    }
  }

  openDelete(costCentre: CostCentreDto): void {
    this.costCentreService.getDeletePreview(costCentre.id).subscribe({
      next: (preview) => {
        this.deletingCostCentre = costCentre;
        this.deletePreview = preview;
        this.cdr.markForCheck();
      },
      error: (err: Error) => {
        this.notificationService.showError('Could not load delete preview: ' + err.message);
      },
    });
  }

  closeDelete(): void {
    this.deletingCostCentre = null;
    this.deletePreview = null;
  }

  confirmDelete(): void {
    if (!this.deletingCostCentre) return;

    this.costCentreService.deleteCostCentre(this.deletingCostCentre.id).subscribe({
      next: (result) => {
        if (result) {
          this.notificationService.showSuccess(
            `Cost centre "${this.deletingCostCentre!.name}" deactivated`,
          );
        } else {
          this.notificationService.showSuccess(
            `Cost centre "${this.deletingCostCentre!.name}" deleted successfully`,
          );
        }
        this.closeDelete();
        this.load();
      },
      error: (err: Error) => {
        this.notificationService.showError('Could not delete cost centre: ' + err.message);
      },
    });
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

  private normalizeBudgetDates(budget: UpsertBudgetEntryDto): UpsertBudgetEntryDto {
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
}
