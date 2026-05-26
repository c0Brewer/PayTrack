import { ChangeDetectorRef, Component, OnInit } from '@angular/core';

import { CostCentreService } from '../../../services/cost-centre/cost-centre-service';
import { NotificationService } from '../../../services/notification/notification-service';
import { CostCentreDto, GetCostCentreOptions } from '../../../types/exporter';
import { StatBoxComponent } from '../../general/boxes/stat-box-component/stat-box-component';
import { PaginationComponent } from '../../general/pagination-component/pagination-component';
import { CostCentreDeleteComponent } from '../cost-centre-delete-component/cost-centre-delete-component';
import { CostCentreEditComponent } from '../cost-centre-edit-component/cost-centre-edit-component';
import { CostCentreFilterComponent } from '../cost-centre-filter-component/cost-centre-filter-component';
import { CostCentreListComponent } from '../cost-centre-list-component/cost-centre-list-component';

@Component({
  selector: 'app-cost-centre-management-component',
  imports: [
    CostCentreListComponent,
    CostCentreFilterComponent,
    CostCentreEditComponent,
    CostCentreDeleteComponent,
    PaginationComponent,
    StatBoxComponent,
  ],
  templateUrl: './cost-centre-management-component.html',
  styleUrl: './cost-centre-management-component.scss',
})
export class CostCentreManagementComponent implements OnInit {
  constructor(
    private readonly costCentreService: CostCentreService,
    private readonly cdr: ChangeDetectorRef,
    private readonly notificationService: NotificationService,
  ) {}

  costCentres: CostCentreDto[] = [];
  editingCostCentre: CostCentreDto | null = null;
  deletingCostCentre: CostCentreDto | null = null;

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
}
