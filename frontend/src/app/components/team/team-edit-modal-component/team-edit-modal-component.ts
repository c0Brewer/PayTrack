import { DatePipe } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';

import {
  BudgetDto,
  BudgetType,
  CostCentreDto,
  SeasonDto,
  TeamDto,
  UpsertTeamBudgetEntryDto,
} from '../../../types/exporter';
import { TeamSaveEvent } from '../../../types/misc-types';
import { ModalComponent } from '../../general/modal-component/modal-component';

interface WorkingBudget {
  originalId: number;
  name: string;
  seasonId: number;
  costCentreId: number;
  targetAmount: number | null;
  periodStart: string;
  periodEnd: string;
  markedForDeletion: boolean;
  type: BudgetType;
}

type BudgetField =
  | 'name'
  | 'costCentreId'
  | 'targetAmount'
  | 'seasonId'
  | 'periodStart'
  | 'periodEnd';
const budgetFields: readonly BudgetField[] = [
  'name',
  'costCentreId',
  'targetAmount',
  'seasonId',
  'periodStart',
  'periodEnd',
];

@Component({
  selector: 'app-team-edit-modal-component',
  imports: [DatePipe, FormsModule, ModalComponent],
  templateUrl: './team-edit-modal-component.html',
  styleUrl: './team-edit-modal-component.scss',
})
export class TeamEditModalComponent implements OnChanges {
  protected readonly BudgetType = BudgetType;
  readonly defaultColor = '#2563eb';
  readonly minNameLength = 3;
  readonly minDescriptionLength = 3;
  readonly predefinedColors = [
    // Neutrals
    '#0f172a',
    '#475569',

    // Blues
    '#1d4ed8',
    '#60a5fa',

    // Cyans
    '#22d3ee',
    '#67e8f9',

    // Greens
    '#059669',
    '#65a30d',
    '#84cc16',

    // Yellows / Ambers
    '#fbbf24',
    '#fcd34d',

    // Oranges
    '#ea580c',
    '#fb923c',

    // Reds
    '#dc2626',
    '#f87171',

    // Purples
    '#7e22ce',
    '#9333ea',
    '#7c3aed',
    '#a78bfa',
    '#c084fc',
    '#c4b5fd',

    // Pinks
    '#ec4899',
    '#f472b6',
    '#f9a8d4',
  ];

  @Input() team: TeamDto = {
    id: -1,
    name: '',
    description: '',
    displayColor: '',
    members: [],
    budgets: [],
  };
  @Input() costCentres: CostCentreDto[] = [];
  @Input() seasons: SeasonDto[] = [];

  @Output() saveEvent = new EventEmitter<TeamSaveEvent>();
  @Output() deleteEvent = new EventEmitter<TeamDto>();
  @Output() closeEvent = new EventEmitter<void>();

  originalTeam: TeamDto | null = null;
  workingBudgets: WorkingBudget[] = [];
  newBudgets: UpsertTeamBudgetEntryDto[] = [];
  newBudgetDraft: UpsertTeamBudgetEntryDto = this.emptyDraft();
  touchedFields: Record<'name' | 'description', boolean> = {
    name: false,
    description: false,
  };
  touchedBudgetFields: Record<BudgetField, boolean> = this.emptyBudgetTouchedFields();

  ngOnChanges(): void {
    if (this.team) {
      // Deep clone to avoid mutating the original reference during editing.
      this.originalTeam = structuredClone(this.team);
      this.workingBudgets = (this.team.budgets ?? []).map((budget: BudgetDto) => ({
        originalId: budget.id,
        name: budget.name,
        seasonId: budget.seasonId,
        costCentreId: budget.costCentreId,
        targetAmount: budget.targetAmount ?? null,
        periodStart: budget.periodStart,
        periodEnd: budget.periodEnd,
        markedForDeletion: false,
        type: (budget.type as BudgetType) ?? BudgetType.Expense,
      }));
      this.newBudgets = [];
      this.newBudgetDraft = this.emptyDraft();
      this.touchedFields = {
        name: false,
        description: false,
      };
      this.touchedBudgetFields = this.emptyBudgetTouchedFields();
    }
  }

  get isCreating(): boolean {
    return this.team.id === -1;
  }

  get selectedColor(): string {
    return this.isHexColor(this.team.displayColor) ? this.team.displayColor : this.defaultColor;
  }

  hasTeamBeenChanged(): boolean {
    if (!this.originalTeam) return false;

    return (
      this.team.name !== this.originalTeam.name ||
      this.team.description !== this.originalTeam.description ||
      this.team.displayColor !== this.originalTeam.displayColor ||
      this.newBudgets.length > 0 ||
      this.workingBudgets.some((budget) => budget.markedForDeletion)
    );
  }

  onSave(): void {
    this.markAllFieldsTouched();

    if (this.isInvalid()) {
      return;
    }

    if (!this.hasTeamBeenChanged()) {
      this.onClose();
      return;
    }

    const budgetIdsToDelete = this.workingBudgets
      .filter((budget) => budget.markedForDeletion)
      .map((budget) => budget.originalId);

    const budgetsToUpsert: UpsertTeamBudgetEntryDto[] = [
      ...this.newBudgets.map((budget) => ({
        ...budget,
        id: null,
      })),
    ];

    this.saveEvent.emit({ team: this.team, budgetsToUpsert, budgetIdsToDelete });
  }

  setDisplayColor(color: string): void {
    const normalizedColor = this.normalizeHexColor(color);

    if (!normalizedColor) {
      return;
    }

    this.team.displayColor = normalizedColor;
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

  onFieldBlur(field: keyof typeof this.touchedFields): void {
    this.touchedFields[field] = true;
  }

  onBudgetFieldBlur(field: BudgetField): void {
    this.touchedBudgetFields[field] = true;
  }

  hasFieldError(field: keyof typeof this.touchedFields): boolean {
    return this.touchedFields[field] && this.getFieldError(field).length > 0;
  }

  getFieldError(field: keyof typeof this.touchedFields): string {
    if (field === 'name') {
      return this.getNameError();
    }

    return this.getDescriptionError();
  }

  hasBudgetFieldError(field: BudgetField): boolean {
    return this.touchedBudgetFields[field] && this.getBudgetFieldError(field).length > 0;
  }

  getBudgetFieldError(field: BudgetField): string {
    switch (field) {
      case 'name':
        return this.newBudgetDraft.name?.trim() ? '' : 'Name is required.';
      case 'costCentreId':
        if (!this.newBudgetDraft.costCentreId) {
          return 'Cost centre is required.';
        }

        if (!this.isCostCentreActive(this.newBudgetDraft.costCentreId)) {
          return 'Select an active cost centre.';
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

  onClose(): void {
    this.closeEvent.emit();
  }

  onDelete(): void {
    if (this.isCreating) return;

    this.deleteEvent.emit(this.team);
  }

  getCostCentreName(costCentreId: number): string {
    return (
      this.costCentres.find((costCentre) => costCentre.id === costCentreId)?.name ??
      `Cost Centre #${costCentreId}`
    );
  }

  getSeasonName(seasonId: number): string {
    return this.seasons.find((season) => season.id === seasonId)?.name ?? `Season #${seasonId}`;
  }

  formatBudgetAmount(amount: number | null | undefined): string {
    if (amount == null) return '—';
    return new Intl.NumberFormat('de-DE', { maximumFractionDigits: 2 }).format(amount);
  }

  getCostCentreOptionLabel(costCentre: CostCentreDto): string {
    return costCentre.isActive === false ? `${costCentre.name} (inactive)` : costCentre.name;
  }

  isCostCentreActive(costCentreId: number): boolean {
    return (
      this.costCentres.find((costCentre) => costCentre.id === costCentreId)?.isActive !== false
    );
  }

  private isHexColor(color: string | null | undefined): color is string {
    return /^#[0-9a-f]{6}$/i.test(color ?? '');
  }

  private normalizeHexColor(color: string | null | undefined): string | null {
    const trimmedColor = color?.trim() ?? '';

    if (!this.isHexColor(trimmedColor)) {
      return null;
    }

    return trimmedColor.toLowerCase();
  }

  private isInvalid(): boolean {
    return this.getNameError().length > 0 || this.getDescriptionError().length > 0;
  }

  private getNameError(): string {
    const nameLength = this.team.name?.trim().length ?? 0;

    if (nameLength === 0) {
      return 'Name is required.';
    }

    if (nameLength < this.minNameLength) {
      return `Name must be at least ${this.minNameLength} characters long.`;
    }

    return '';
  }

  private getDescriptionError(): string {
    const descriptionLength = this.team.description?.trim().length ?? 0;

    if (descriptionLength > 0 && descriptionLength < this.minDescriptionLength) {
      return `Description must be at least ${this.minDescriptionLength} characters long.`;
    }

    return '';
  }

  private markAllFieldsTouched(): void {
    this.touchedFields.name = true;
    this.touchedFields.description = true;
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

  private emptyDraft(): UpsertTeamBudgetEntryDto {
    return {
      id: null,
      name: '',
      description: null,
      costCentreId: 0,
      seasonId: 0,
      targetAmount: null,
      periodStart: '',
      periodEnd: '',
      type: BudgetType.Expense,
    };
  }
}
