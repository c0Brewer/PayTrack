import { DatePipe } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';

import {
  BudgetDto,
  CostCentreDto,
  TeamDto,
  UpsertTeamBudgetEntryDto,
} from '../../../types/exporter';
import { TeamSaveEvent } from '../../../types/misc-types';
import { ModalComponent } from '../../general/modal-component/modal-component';

interface WorkingBudget {
  originalId: number;
  costCentreId: number;
  targetAmount: number;
  periodStart: string;
  periodEnd: string;
  markedForDeletion: boolean;
}

@Component({
  selector: 'app-team-edit-modal-component',
  imports: [DatePipe, FormsModule, ModalComponent],
  templateUrl: './team-edit-modal-component.html',
  styleUrl: './team-edit-modal-component.scss',
})
export class TeamEditModalComponent implements OnChanges {
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

  ngOnChanges(): void {
    if (this.team) {
      // Deep clone to avoid mutating the original reference during editing.
      this.originalTeam = structuredClone(this.team);
      this.workingBudgets = (this.team.budgets ?? []).map((budget: BudgetDto) => ({
        originalId: budget.id,
        costCentreId: budget.costCentreId,
        targetAmount: budget.targetAmount,
        periodStart: budget.periodStart,
        periodEnd: budget.periodEnd,
        markedForDeletion: false,
      }));
      this.newBudgets = [];
      this.newBudgetDraft = this.emptyDraft();
      this.touchedFields = {
        name: false,
        description: false,
      };
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
    if (
      !this.newBudgetDraft.costCentreId ||
      !this.isCostCentreActive(this.newBudgetDraft.costCentreId) ||
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

  onFieldBlur(field: keyof typeof this.touchedFields): void {
    this.touchedFields[field] = true;
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

  formatBudgetAmount(amount: number): string {
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

  private emptyDraft(): UpsertTeamBudgetEntryDto {
    return { id: null, costCentreId: 0, targetAmount: 0, periodStart: '', periodEnd: '' };
  }
}
