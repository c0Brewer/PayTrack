import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

import { TeamService } from '../../../services/team-service';
import { TeamDto } from '../../../types/exporter';

@Component({
  selector: 'app-team-list-component',
  imports: [CommonModule],
  templateUrl: './team-list-component.html',
  styleUrl: './team-list-component.scss',
})
export class TeamListComponent {
  teams: TeamDto[] = [];

  constructor(private teamService: TeamService) {}

  ngOnInit(): void {
    this.loadTeams();
  }

  loadTeams(): void {
    this.teamService.getTeams().subscribe({
      next: (data) => (this.teams = data),
      error: (err) => console.error(err),
    });
  }
}
