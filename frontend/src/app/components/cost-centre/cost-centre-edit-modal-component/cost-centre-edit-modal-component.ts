import { DatePipe } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';

import {
  BudgetDto,
  CostCentreDto,
  CostCentreSaveEvent,
  UpsertBudgetEntryDto,
} from '../../../types/exporter';
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
  selector: 'app-cost-centre-edit-modal-component',
  imports: [DatePipe, FormsModule, ModalComponent],
  templateUrl: './cost-centre-edit-modal-component.html',
  styleUrl: './cost-centre-edit-modal-component.scss',
})
export class CostCentreEditModalComponent implements OnChanges {
  @Input() costCentre: CostCentreDto = {
    id: -1,
    name: '',
    description: null,
    displayColor: null,
    budgets: [],
  };

  @Output() saveEvent = new EventEmitter<CostCentreSaveEvent>();
  @Output() closeEvent = new EventEmitter<void>();

  originalCostCentre: CostCentreDto | null = null;
  workingBudgets: WorkingBudget[] = [];
  newBudgets: UpsertBudgetEntryDto[] = [];
  newBudgetDraft: UpsertBudgetEntryDto = this.emptyDraft();

  ngOnChanges(): void {
    if (this.costCentre) {
      this.originalCostCentre = structuredClone(this.costCentre);
      this.workingBudgets = this.costCentre.budgets.map((b: BudgetDto) => ({
        originalId: b.id,
        teamId: b.teamId,
        targetAmount: b.targetAmount,
        periodStart: b.periodStart,
        periodEnd: b.periodEnd,
        markedForDeletion: false,
      }));
      this.newBudgets = [];
      this.newBudgetDraft = this.emptyDraft();
    }
  }

  get isCreating(): boolean {
    return this.costCentre.id === -1;
  }

  hasChanged(): boolean {
    if (!this.originalCostCentre) return false;
    const fieldsChanged =
      this.costCentre.name !== this.originalCostCentre.name ||
      this.costCentre.description !== this.originalCostCentre.description ||
      this.costCentre.displayColor !== this.originalCostCentre.displayColor;
    const budgetsChanged =
      this.newBudgets.length > 0 || this.workingBudgets.some((b) => b.markedForDeletion);
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

  onSave(): void {
    if (!this.isCreating && !this.hasChanged()) {
      this.onClose();
      return;
    }

    const budgetIdsToDelete = this.workingBudgets
      .filter((b) => b.markedForDeletion)
      .map((b) => b.originalId);

    const budgetsToUpsert: UpsertBudgetEntryDto[] = this.newBudgets.map((b) => ({
      ...b,
      id: null,
    }));

    this.saveEvent.emit({ costCentre: this.costCentre, budgetsToUpsert, budgetIdsToDelete });
  }

  onClose(): void {
    this.closeEvent.emit();
  }

  private emptyDraft(): UpsertBudgetEntryDto {
    return { id: null, teamId: 0, targetAmount: 0, periodStart: '', periodEnd: '' };
  }
}
