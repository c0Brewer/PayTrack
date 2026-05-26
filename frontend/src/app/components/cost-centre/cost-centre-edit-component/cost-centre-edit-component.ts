import { DatePipe } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { CostCentreService } from '../../../services/cost-centre/cost-centre-service';
import { NotificationService } from '../../../services/notification/notification-service';
import {
  BudgetDto,
  CostCentreDto,
  CreateCostCentreRequestDto,
  UpdateCostCentreRequestDto,
  UpsertBudgetEntryDto,
} from '../../../types/exporter';
import { CostCentreSaveEvent } from '../../../types/misc-types';
import { ModalComponent } from '../../general/modal-component/modal-component';

interface WorkingBudget {
  originalId: number;
  teamId: number;
  targetAmount: number;
  periodStart: string;
  periodEnd: string;
  markedForDeletion: boolean;
}

@Component({
  selector: 'app-cost-centre-edit-component',
  imports: [DatePipe, FormsModule, ModalComponent],
  templateUrl: './cost-centre-edit-component.html',
  styleUrl: './cost-centre-edit-component.scss',
})
export class CostCentreEditComponent implements OnChanges {
  constructor(
    private readonly costCentreService: CostCentreService,
    private readonly notificationService: NotificationService,
  ) {}

  @Input() costCentre: CostCentreDto | null = null;

  @Output() saveEvent = new EventEmitter<void>();
  @Output() closeEvent = new EventEmitter<void>();

  editingCostCentre: CostCentreDto | null = null;
  originalCostCentre: CostCentreDto | null = null;
  workingBudgets: WorkingBudget[] = [];
  newBudgets: UpsertBudgetEntryDto[] = [];
  newBudgetDraft: UpsertBudgetEntryDto = this.emptyDraft();

  ngOnChanges(): void {
    if (!this.costCentre) {
      this.resetEditState();
      return;
    }

    this.editingCostCentre = structuredClone(this.costCentre);
    this.prepareEditState(this.editingCostCentre);
  }

  get isCreating(): boolean {
    return this.editingCostCentre?.id === -1;
  }

  hasChanged(): boolean {
    if (!this.editingCostCentre || !this.originalCostCentre) return false;

    const fieldsChanged =
      this.editingCostCentre.name !== this.originalCostCentre.name ||
      this.editingCostCentre.description !== this.originalCostCentre.description ||
      this.editingCostCentre.displayColor !== this.originalCostCentre.displayColor;
    const budgetsChanged =
      this.newBudgets.length > 0 || this.workingBudgets.some((budget) => budget.markedForDeletion);

    return fieldsChanged || budgetsChanged;
  }

  toggleBudgetDeletion(budget: WorkingBudget): void {
    budget.markedForDeletion = !budget.markedForDeletion;
  }

  addNewBudget(): void {
    if (
      !this.newBudgetDraft.teamId ||
      !this.newBudgetDraft.periodStart ||
      !this.newBudgetDraft.periodEnd
    ) {
      return;
    }

    this.newBudgets.push({ ...this.newBudgetDraft });
    this.newBudgetDraft = this.emptyDraft();
  }

  removeNewBudget(index: number): void {
    this.newBudgets.splice(index, 1);
  }

  onClose(): void {
    this.closeEvent.emit();
  }

  saveEdit(): void {
    if (!this.editingCostCentre) return;

    if (!this.isCreating && !this.hasChanged()) {
      this.onClose();
      return;
    }

    const budgetIdsToDelete = this.workingBudgets
      .filter((budget) => budget.markedForDeletion)
      .map((budget) => budget.originalId);

    const budgetsToUpsert: UpsertBudgetEntryDto[] = this.newBudgets.map((budget) => ({
      ...budget,
      id: null,
    }));

    this.save({ costCentre: this.editingCostCentre, budgetsToUpsert, budgetIdsToDelete });
  }

  private save(event: CostCentreSaveEvent): void {
    const { costCentre, budgetsToUpsert, budgetIdsToDelete } = event;

    if (costCentre.id === -1) {
      this.createCostCentre(costCentre, budgetsToUpsert);
      return;
    }

    this.updateCostCentre(costCentre, budgetsToUpsert, budgetIdsToDelete);
  }

  private createCostCentre(
    costCentre: CostCentreDto,
    budgetsToUpsert: UpsertBudgetEntryDto[],
  ): void {
    const request: CreateCostCentreRequestDto = {
      name: costCentre.name,
      description: costCentre.description ?? undefined,
      displayColor: costCentre.displayColor ?? undefined,
      budgets:
        budgetsToUpsert.length > 0
          ? budgetsToUpsert.map(({ teamId, targetAmount, periodStart, periodEnd }) => ({
              teamId,
              targetAmount,
              periodStart,
              periodEnd,
            }))
          : undefined,
    };

    this.costCentreService.createCostCentre(request).subscribe({
      next: () => {
        this.notificationService.showSuccess('Cost centre created successfully');
        this.saveEvent.emit();
      },
      error: (err: Error) => {
        this.notificationService.showError('Could not create cost centre: ' + err.message);
      },
    });
  }

  private updateCostCentre(
    costCentre: CostCentreDto,
    budgetsToUpsert: UpsertBudgetEntryDto[],
    budgetIdsToDelete: number[],
  ): void {
    const request: UpdateCostCentreRequestDto = {
      name: costCentre.name,
      description: costCentre.description ?? undefined,
      displayColor: costCentre.displayColor ?? undefined,
      budgetsToUpsert: budgetsToUpsert.length > 0 ? budgetsToUpsert : undefined,
      budgetIdsToDelete: budgetIdsToDelete.length > 0 ? budgetIdsToDelete : undefined,
    };

    this.costCentreService.updateCostCentre(costCentre.id, request).subscribe({
      next: () => {
        this.notificationService.showSuccess('Cost centre updated successfully');
        this.saveEvent.emit();
      },
      error: (err: Error) => {
        this.notificationService.showError('Could not update cost centre: ' + err.message);
      },
    });
  }

  private prepareEditState(costCentre: CostCentreDto): void {
    this.originalCostCentre = structuredClone(costCentre);
    this.workingBudgets = (costCentre.budgets ?? []).map((budget: BudgetDto) => ({
      originalId: budget.id,
      teamId: budget.teamId,
      targetAmount: budget.targetAmount,
      periodStart: budget.periodStart,
      periodEnd: budget.periodEnd,
      markedForDeletion: false,
    }));
    this.newBudgets = [];
    this.newBudgetDraft = this.emptyDraft();
  }

  private resetEditState(): void {
    this.editingCostCentre = null;
    this.originalCostCentre = null;
    this.workingBudgets = [];
    this.newBudgets = [];
    this.newBudgetDraft = this.emptyDraft();
  }

  private emptyDraft(): UpsertBudgetEntryDto {
    return { id: null, teamId: 0, targetAmount: 0, periodStart: '', periodEnd: '' };
  }
}
