import { CommonModule } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';

import { NotificationService } from '../../../services/notification/notification-service';
import { TeamService } from '../../../services/team/team-service';
import { TeamDto } from '../../../types/exporter';
import {NavbarComponent} from '../../navbar/navbar-component/navbar-component';

@Component({
  selector: 'app-team-list-component',
  imports: [CommonModule, NavbarComponent],
  templateUrl: './team-list-component.html',
  styleUrl: './team-list-component.scss',
})
export class TeamListComponent implements OnInit {
  teams = signal<TeamDto[]>([]);

  constructor(
    private readonly teamService: TeamService,
    private readonly notificationService: NotificationService,
  ) {}

  ngOnInit(): void {
    this.loadTeams();
  }

  loadTeams(): void {
    this.teamService.getTeams().subscribe({
      next: (data) => {
        this.teams.set(data);
      },
      error: (err) => {
        this.notificationService.showError(err);
      },
    });
  }
}
