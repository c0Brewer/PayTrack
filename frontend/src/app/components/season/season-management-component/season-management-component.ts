import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { NotificationService } from '../../../services/notification/notification-service';
import { SeasonService } from '../../../services/season/season-service';
import { SeasonDto } from '../../../types/exporter';

@Component({
  selector: 'app-season-management-component',
  imports: [FormsModule],
  templateUrl: './season-management-component.html',
  styleUrl: './season-management-component.scss',
})
export class SeasonManagementComponent implements OnInit {
  constructor(
    private readonly seasonService: SeasonService,
    private readonly cdr: ChangeDetectorRef,
    private readonly notificationService: NotificationService,
  ) {}

  seasons: SeasonDto[] = [];
  newSeasonName = '';

  ngOnInit(): void {
    this.loadSeasons();
  }

  loadSeasons(): void {
    this.seasonService.getSeasons().subscribe({
      next: (seasons) => {
        this.seasons = seasons;
        this.cdr.markForCheck();
      },
      error: (err: Error) => {
        this.notificationService.showError('Could not load seasons: ' + err.message);
      },
    });
  }

  createSeason(): void {
    const name = this.newSeasonName.trim();
    if (!name) {
      return;
    }

    this.seasonService.createSeason({ name }).subscribe({
      next: () => {
        this.notificationService.showSuccess('Season created successfully');
        this.newSeasonName = '';
        this.loadSeasons();
      },
      error: (err: Error) => {
        this.notificationService.showError('Could not create season: ' + err.message);
      },
    });
  }
}
