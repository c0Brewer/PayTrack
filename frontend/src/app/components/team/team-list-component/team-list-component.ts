import { Component, EventEmitter, Input, Output } from '@angular/core';

import { CostCentreDto, TeamDto } from '../../../types/exporter';

type TeamBudget = NonNullable<TeamDto['budgets']>[number];

@Component({
  selector: 'app-team-list-component',
  templateUrl: './team-list-component.html',
  styleUrl: './team-list-component.scss',
})
export class TeamListComponent {
  @Input() teams: TeamDto[] = [];
  @Input() costCentres: CostCentreDto[] = [];

  @Output() openEditTeam = new EventEmitter<TeamDto>();

  private readonly expandedBudgetTeamIds = new Set<number>();

  onOpenEditTeam(team: TeamDto): void {
    this.openEditTeam.emit(team);
  }

  getDescription(team: TeamDto): string {
    return team.description?.trim() || 'No description';
  }

  getDisplayColor(team: TeamDto): string {
    return team.displayColor?.trim() || 'transparent';
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

  getBudgetDisplayValue(budget: TeamBudget): string {
    return `${this.getCostCentreName(budget.costCentreId)}: ${this.formatBudgetAmount(budget.targetAmount)} €`;
  }

  formatBudgetAmount(amount: number): string {
    return new Intl.NumberFormat('de-DE', { maximumFractionDigits: 2 }).format(amount);
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

  getTeamNameTextColor(team: TeamDto): string {
    const darkTeamTextColor = `#111827`;
    const lightTeamTextColor = `#F9FAFB`;

    const rgb = hexToRgb(team.displayColor?.trim() || 'transparent');
    if (rgb == null) return darkTeamTextColor;
    const brightness = (rgb.red * 299 + rgb.green * 587 + rgb.blue * 114) / 1000;
    return brightness >= 160 ? darkTeamTextColor : lightTeamTextColor;
  }

  getVisibleColumnCount(): number {
    return 6;
  }
}

function hexToRgb(color: string): { red: number; green: number; blue: number } | null {
  if (!/^#[0-9a-fA-F]{6}$/.test(color)) {
    return null;
  }

  return {
    red: Number.parseInt(color.slice(1, 3), 16),
    green: Number.parseInt(color.slice(3, 5), 16),
    blue: Number.parseInt(color.slice(5, 7), 16),
  };
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
