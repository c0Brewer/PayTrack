import { Component, EventEmitter, Input, Output } from '@angular/core';
import { RouterLink } from '@angular/router';

import { CostCentreDto, TeamDto } from '../../../types/exporter';

type TeamBudget = NonNullable<TeamDto['budgets']>[number];

@Component({
  selector: 'app-team-list-component',
  imports: [RouterLink],
  templateUrl: './team-list-component.html',
  styleUrl: './team-list-component.scss',
})
export class TeamListComponent {
  @Input() teams: TeamDto[] = [];
  @Input() costCentres: CostCentreDto[] = [];
  @Input() activeStatusPendingIds: ReadonlySet<number> = new Set<number>();

  @Output() openEditTeam = new EventEmitter<TeamDto>();
  @Output() toggleActive = new EventEmitter<TeamDto>();

  private readonly expandedBudgetTeamIds = new Set<number>();

  onOpenEditTeam(team: TeamDto): void {
    this.openEditTeam.emit(team);
  }

  getDescription(team: TeamDto): string {
    return team.description?.trim() || 'No description';
  }

  getDisplayColor(team: TeamDto): string {
    return team.displayColor?.trim() || '#f47f1f';
  }

  getMembersCount(team: TeamDto): number {
    return team.members?.length ?? 0;
  }

  getCurrentBudgets(team: TeamDto): TeamBudget[] {
    const currentTime = new Date();
    return team.budgets?.filter((budget) => isDateWithinBudgetPeriod(budget, currentTime)) ?? [];
  }

  getVisibleCurrentBudgets(team: TeamDto): TeamBudget[] {
    const currentBudgets = this.getCurrentBudgets(team);
    return this.isBudgetListExpanded(team) ? currentBudgets : currentBudgets.slice(0, 3);
  }

  getHiddenCurrentBudgetCount(team: TeamDto): number {
    return Math.max(this.getCurrentBudgets(team).length - 3, 0);
  }

  onToggleActive(user: TeamDto): void {
    this.toggleActive.emit(user);
  }

  isActiveStatusPending(user: TeamDto): boolean {
    return this.activeStatusPendingIds.has(user.id);
  }

  getBudgetDisplayValue(budget: TeamBudget): string {
    const formatted =
      budget.targetAmount == null
        ? '—'
        : new Intl.NumberFormat('de-DE', { style: 'currency', currency: 'EUR' }).format(
            budget.targetAmount,
          );
    return `${this.getCostCentreName(budget.costCentreId)}: ${formatted}`;
  }

  hasHiddenCurrentBudgets(team: TeamDto): boolean {
    return this.getHiddenCurrentBudgetCount(team) > 0;
  }

  isBudgetListExpanded(team: TeamDto): boolean {
    return this.expandedBudgetTeamIds.has(team.id);
  }

  toggleBudgetList(team: TeamDto): void {
    if (this.isBudgetListExpanded(team)) {
      this.expandedBudgetTeamIds.delete(team.id);
      return;
    }

    this.expandedBudgetTeamIds.add(team.id);
  }

  private getCostCentreName(costCentreId: number): string {
    return (
      this.costCentres.find((costCentre) => costCentre.id === costCentreId)?.name ??
      `Cost centre #${costCentreId}`
    );
  }

  getVisibleColumnCount(): number {
    return 6;
  }
}

function isDateWithinBudgetPeriod(budget: TeamBudget, currentTime: Date): boolean {
  const periodStart = toTimestamp(budget.periodStart);
  const periodEnd = toTimestamp(budget.periodEnd);

  if (periodStart == null || periodEnd == null) {
    return false;
  }

  const currentTimestamp = currentTime.getTime();
  return currentTimestamp >= periodStart && currentTimestamp <= periodEnd;
}

function toTimestamp(value: Date | string): number | null {
  const timestamp = value instanceof Date ? value.getTime() : new Date(value).getTime();
  return Number.isNaN(timestamp) ? null : timestamp;
}
