import { ChangeDetectorRef, Component, OnInit } from '@angular/core';

import { CostCentreService } from '../../../services/cost-centre/cost-centre-service';
import { NotificationService } from '../../../services/notification/notification-service';
import {
  CostCentreDto,
  CreateCostCentreRequestDto,
  DeleteCostCentrePreviewDto,
  UpdateCostCentreRequestDto,
} from '../../../types/exporter';
import { CostCentreDeletePreviewModalComponent } from '../cost-centre-delete-preview-modal-component/cost-centre-delete-preview-modal-component';
import { CostCentreEditModalComponent } from '../cost-centre-edit-modal-component/cost-centre-edit-modal-component';
import { CostCentreListComponent } from '../cost-centre-list-component/cost-centre-list-component';

@Component({
  selector: 'app-cost-centre-management-component',
  imports: [
    CostCentreListComponent,
    CostCentreEditModalComponent,
    CostCentreDeletePreviewModalComponent,
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
  deletePreview: DeleteCostCentrePreviewDto | null = null;

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.costCentreService.getCostCentres().subscribe({
      next: (data) => {
        this.costCentres = data;
        this.cdr.markForCheck();
      },
      error: (err: Error) => {
        this.notificationService.showError('Could not load cost centres: ' + err.message);
      },
    });
  }

  openCreate(): void {
    this.editingCostCentre = {
      id: -1,
      name: '',
      description: null,
      displayColor: null,
      budgets: [],
    };
  }

  openEdit(costCentre: CostCentreDto): void {
    this.editingCostCentre = structuredClone(costCentre);
  }

  closeEdit(): void {
    this.editingCostCentre = null;
  }

  save(costCentre: CostCentreDto): void {
    if (costCentre.id === -1) {
      const request: CreateCostCentreRequestDto = {
        name: costCentre.name,
        description: costCentre.description ?? undefined,
        displayColor: costCentre.displayColor ?? undefined,
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
      next: () => {
        this.notificationService.showSuccess(
          `Cost centre "${this.deletingCostCentre!.name}" deleted successfully`,
        );
        this.closeDelete();
        this.load();
      },
      error: (err: Error) => {
        this.notificationService.showError('Could not delete cost centre: ' + err.message);
      },
    });
  }
}
