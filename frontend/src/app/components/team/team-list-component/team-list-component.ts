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

  getBudgetsCount(team: TeamDto): number {
    return team.budgets?.length ?? 0;
  }
}
