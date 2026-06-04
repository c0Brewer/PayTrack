import { DatePipe } from '@angular/common';
import { EuroPipe } from '../../../pipes/euro.pipe';
import { Component, EventEmitter, Input, OnChanges, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';

import {
  BudgetDto,
  BudgetType,
  CostCentreDto,
  SeasonDto,
  TeamDto,
  UpsertBudgetEntryDto,
} from '../../../types/exporter';
import { CostCentreSaveEvent } from '../../../types/misc-types';
import { ModalComponent } from '../../general/modal-component/modal-component';

interface WorkingBudget {
  originalId: number;
  name: string;
  seasonId: number;
  teamId: number;
  targetAmount: number | null;
  periodStart: string;
  periodEnd: string;
  markedForDeletion: boolean;
  type: BudgetType;
}

type BudgetField = 'name' | 'teamId' | 'targetAmount' | 'seasonId' | 'periodStart' | 'periodEnd';
const budgetFields: readonly BudgetField[] = [
  'name',
  'teamId',
  'targetAmount',
  'seasonId',
  'periodStart',
  'periodEnd',
];

@Component({
  selector: 'app-cost-centre-edit-modal-component',
  imports: [DatePipe, EuroPipe, FormsModule, ModalComponent],
  templateUrl: './cost-centre-edit-modal-component.html',
  styleUrl: './cost-centre-edit-modal-component.scss',
})
export class CostCentreEditModalComponent implements OnChanges {
  protected readonly BudgetType = BudgetType;

  @Input() costCentre: CostCentreDto = {
    id: -1,
    name: '',
    description: null,
    displayColor: null,
    budgets: [],
    isActive: true,
  };
  @Input() teams: TeamDto[] = [];
  @Input() seasons: SeasonDto[] = [];

  @Output() saveEvent = new EventEmitter<CostCentreSaveEvent>();
  @Output() closeEvent = new EventEmitter<void>();

  originalCostCentre: CostCentreDto | null = null;
  workingBudgets: WorkingBudget[] = [];
  newBudgets: UpsertBudgetEntryDto[] = [];
  newBudgetDraft: UpsertBudgetEntryDto = this.emptyDraft();
  touchedBudgetFields: Record<BudgetField, boolean> = this.emptyBudgetTouchedFields();

  ngOnChanges(): void {
    if (this.costCentre) {
      this.originalCostCentre = structuredClone(this.costCentre);
      this.workingBudgets = (this.costCentre.budgets ?? []).map((b: BudgetDto) => ({
        originalId: b.id,
        name: b.name,
        seasonId: b.seasonId,
        teamId: b.teamId,
        targetAmount: b.targetAmount ?? null,
        periodStart: b.periodStart,
        periodEnd: b.periodEnd,
        markedForDeletion: false,
        type: (b.type as BudgetType) ?? BudgetType.Expense,
      }));
      this.newBudgets = [];
      this.newBudgetDraft = this.emptyDraft();
      this.touchedBudgetFields = this.emptyBudgetTouchedFields();
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
    this.markAllBudgetFieldsTouched();

    if (this.isBudgetDraftInvalid()) {
      return;
    }

    this.newBudgets.push({ ...this.newBudgetDraft });
    this.newBudgetDraft = this.emptyDraft();
    this.touchedBudgetFields = this.emptyBudgetTouchedFields();
  }

  removeNewBudget(index: number): void {
    this.newBudgets.splice(index, 1);
  }

  getTeamOptionLabel(team: TeamDto): string {
    return team.isActive === false ? `${team.name} (inactive)` : team.name;
  }

  getTeamName(teamId: number): string {
    return this.teams.find((team) => team.id === teamId)?.name ?? `Team #${teamId}`;
  }

  getSeasonName(seasonId: number): string {
    return this.seasons.find((season) => season.id === seasonId)?.name ?? `Season #${seasonId}`;
  }

  isTeamActive(teamId: number): boolean {
    return this.teams.find((team) => team.id === teamId)?.isActive !== false;
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

  onBudgetFieldBlur(field: BudgetField): void {
    this.touchedBudgetFields[field] = true;
  }

  hasBudgetFieldError(field: BudgetField): boolean {
    return this.touchedBudgetFields[field] && this.getBudgetFieldError(field).length > 0;
  }

  getBudgetFieldError(field: BudgetField): string {
    switch (field) {
      case 'name':
        return this.newBudgetDraft.name?.trim() ? '' : 'Name is required.';
      case 'teamId':
        if (!this.newBudgetDraft.teamId) {
          return 'Team is required.';
        }

        if (!this.isTeamActive(this.newBudgetDraft.teamId)) {
          return 'Select an active team.';
        }

        return '';
      case 'targetAmount':
        if (this.newBudgetDraft.type === BudgetType.Income) {
          return '';
        }

        if (
          this.newBudgetDraft.targetAmount === null ||
          this.newBudgetDraft.targetAmount === undefined ||
          Number.isNaN(Number(this.newBudgetDraft.targetAmount))
        ) {
          return 'Amount is required.';
        }

        if (Number(this.newBudgetDraft.targetAmount) < 0) {
          return 'Amount must be non-negative.';
        }

        return '';
      case 'seasonId':
        return this.newBudgetDraft.seasonId ? '' : 'Season is required.';
      case 'periodStart':
        return this.newBudgetDraft.periodStart ? '' : 'Period start is required.';
      case 'periodEnd':
        if (!this.newBudgetDraft.periodEnd) {
          return 'Period end is required.';
        }

        if (
          this.newBudgetDraft.periodStart &&
          this.newBudgetDraft.periodEnd < this.newBudgetDraft.periodStart
        ) {
          return 'Period end must not be before period start.';
        }

        return '';
    }
  }

  private markAllBudgetFieldsTouched(): void {
    budgetFields.forEach((field) => {
      this.touchedBudgetFields[field] = true;
    });
  }

  private isBudgetDraftInvalid(): boolean {
    return budgetFields.some((field) => this.getBudgetFieldError(field).length > 0);
  }

  private emptyBudgetTouchedFields(): Record<BudgetField, boolean> {
    return Object.fromEntries(budgetFields.map((field) => [field, false])) as Record<
      BudgetField,
      boolean
    >;
  }

  private emptyDraft(): UpsertBudgetEntryDto {
    return {
      id: null,
      name: '',
      description: null,
      teamId: 0,
      seasonId: 0,
      targetAmount: null,
      periodStart: '',
      periodEnd: '',
      type: BudgetType.Expense,
    };
  }
}
