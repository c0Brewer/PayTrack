import { DatePipe } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { CostCentreService } from '../../../services/cost-centre/cost-centre-service';
import { NotificationService } from '../../../services/notification/notification-service';
import {
  BudgetDto,
  CostCentreDto,
  CreateCostCentreRequestDto,
  SeasonDto,
  TeamDto,
  UpdateCostCentreRequestDto,
  UpsertBudgetEntryDto,
} from '../../../types/exporter';
import { CostCentreSaveEvent } from '../../../types/misc-types';
import { ModalComponent } from '../../general/modal-component/modal-component';

interface WorkingBudget {
  originalId: number;
  name: string;
  seasonId: number;
  teamId: number;
  targetAmount: number;
  periodStart: string;
  periodEnd: string;
  markedForDeletion: boolean;
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
  @Input() teams: TeamDto[] = [];
  @Input() seasons: SeasonDto[] = [];

  @Output() saveEvent = new EventEmitter<void>();
  @Output() closeEvent = new EventEmitter<void>();

  editingCostCentre: CostCentreDto | null = null;
  originalCostCentre: CostCentreDto | null = null;
  workingBudgets: WorkingBudget[] = [];
  newBudgets: UpsertBudgetEntryDto[] = [];
  newBudgetDraft: UpsertBudgetEntryDto = this.emptyDraft();
  touchedBudgetFields: Record<BudgetField, boolean> = this.emptyBudgetTouchedFields();

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
        if (!this.newBudgetDraft.teamId) return 'Team is required.';
        return this.isTeamActive(this.newBudgetDraft.teamId) ? '' : 'Select an active team.';
      case 'targetAmount':
        if (
          this.newBudgetDraft.targetAmount === null ||
          this.newBudgetDraft.targetAmount === undefined ||
          Number.isNaN(Number(this.newBudgetDraft.targetAmount))
        ) {
          return 'Amount is required.';
        }

        return Number(this.newBudgetDraft.targetAmount) < 0 ? 'Amount must be non-negative.' : '';
      case 'seasonId':
        return this.newBudgetDraft.seasonId ? '' : 'Season is required.';
      case 'periodStart':
        return this.newBudgetDraft.periodStart ? '' : 'Period start is required.';
      case 'periodEnd':
        if (!this.newBudgetDraft.periodEnd) return 'Period end is required.';
        return this.newBudgetDraft.periodStart &&
          this.newBudgetDraft.periodEnd < this.newBudgetDraft.periodStart
          ? 'Period end must not be before period start.'
          : '';
    }
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
          ? budgetsToUpsert.map(
              ({ name, description, teamId, seasonId, targetAmount, periodStart, periodEnd }) => ({
                name,
                description,
                teamId,
                seasonId,
                targetAmount,
                periodStart: this.toApiDateTime(periodStart),
                periodEnd: this.toApiDateTime(periodEnd),
              }),
            )
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
      budgetsToUpsert:
        budgetsToUpsert.length > 0
          ? budgetsToUpsert.map((budget) => this.normalizeBudgetDates(budget))
          : undefined,
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

  private prepareEditState(costCentre: CostCentreDto): void {
    this.originalCostCentre = structuredClone(costCentre);
    this.workingBudgets = (costCentre.budgets ?? []).map((budget: BudgetDto) => ({
      originalId: budget.id,
      name: budget.name,
      seasonId: budget.seasonId,
      teamId: budget.teamId,
      targetAmount: budget.targetAmount,
      periodStart: budget.periodStart,
      periodEnd: budget.periodEnd,
      markedForDeletion: false,
    }));
    this.newBudgets = [];
    this.newBudgetDraft = this.emptyDraft();
    this.touchedBudgetFields = this.emptyBudgetTouchedFields();
  }

  private resetEditState(): void {
    this.editingCostCentre = null;
    this.originalCostCentre = null;
    this.workingBudgets = [];
    this.newBudgets = [];
    this.newBudgetDraft = this.emptyDraft();
    this.touchedBudgetFields = this.emptyBudgetTouchedFields();
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
      targetAmount: 0,
      periodStart: '',
      periodEnd: '',
    };
  }
}
