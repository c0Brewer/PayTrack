import { CommonModule } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';

import { TeamService } from '../../../services/team/team-service';
import { TeamDto } from '../../../types/exporter';

@Component({
  selector: 'app-team-list-component',
  imports: [CommonModule],
  templateUrl: './team-list-component.html',
  styleUrl: './team-list-component.scss',
})
export class TeamListComponent implements OnInit {
  teams = signal<TeamDto[]>([]);

  constructor(private readonly teamService: TeamService) {}

  ngOnInit(): void {
    this.loadTeams();
  }

  loadTeams(): void {
    this.teamService.getTeams().subscribe({
      next: (data) => {
        this.teams.set(data);
      },
      error: (err) => console.error(err),
    });
  }
}
