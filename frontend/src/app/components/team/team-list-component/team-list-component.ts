import { Component, EventEmitter, Input, Output } from '@angular/core';

import { TeamDto } from '../../../types/exporter';

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

  getBudgetTargetAmount(team: TeamDto): number | null {
    const budgetValue = team.budget as
      | { targetAmount: number }
      | { targetAmount: number }[]
      | null
      | undefined;
    if (budgetValue == null) {
      return null;
    }

    const budget = Array.isArray(budgetValue) ? budgetValue[0] : budgetValue;
    return budget?.targetAmount ?? null;
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
