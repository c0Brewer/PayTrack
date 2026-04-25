import { Component, EventEmitter, Input, Output } from '@angular/core';

import { TeamDto } from '../../../types/exporter';

type TeamBudget = NonNullable<TeamDto['budgets']>[number];

@Component({
  selector: 'app-team-list-component',
  imports: [],
  templateUrl: './team-list-component.html',
  styleUrl: './team-list-component.scss',
})
export class TeamListComponent {
  @Input() teams: TeamDto[] = [];

  @Output() openEditTeam = new EventEmitter<TeamDto>();

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

  getCurrentBudget(team: TeamDto): TeamBudget | null {
    const currentDate = toDateKey(new Date());
    if (currentDate == null) {
      return null;
    }

    return team.budgets?.find((budget) => isDateWithinBudgetPeriod(budget, currentDate)) ?? null;
  }

  getBudgetTargetAmount(team: TeamDto): number | null {
    return this.getCurrentBudget(team)?.targetAmount ?? null;
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
    return 5;
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

function isDateWithinBudgetPeriod(budget: TeamBudget, currentDate: string): boolean {
  const periodStart = toDateKey(budget.periodStart);
  const periodEnd = toDateKey(budget.periodEnd);

  if (periodStart == null || periodEnd == null) {
    return false;
  }

  return currentDate >= periodStart && currentDate <= periodEnd;
}

function toDateKey(value: Date | string): string | null {
  if (value instanceof Date) {
    const year = value.getFullYear();
    const month = `${value.getMonth() + 1}`.padStart(2, '0');
    const day = `${value.getDate()}`.padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  const dateMatch = value.match(/^\d{4}-\d{2}-\d{2}/);
  return dateMatch?.[0] ?? null;
}
