import { ChangeDetectorRef, Component, OnInit } from '@angular/core';

import { NotificationService } from '../../../services/notification/notification-service';
import { SeasonService } from '../../../services/season/season-service';
import { SeasonDto } from '../../../types/exporter';
import { ModalComponent } from '../../general/modal-component/modal-component';
import { SeasonFormComponent } from '../season-form-component/season-form-component';
import { SeasonListComponent } from '../season-list-component/season-list-component';

@Component({
  selector: 'app-season-management-component',
  imports: [ModalComponent, SeasonFormComponent, SeasonListComponent],
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
  seasonToDelete: SeasonDto | null = null;

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

  createSeason(name: string): void {
    this.seasonService.createSeason({ name }).subscribe({
      next: () => {
        this.notificationService.showSuccess('Season created successfully');
        this.loadSeasons();
      },
      error: (err: Error) => {
        this.notificationService.showError('Could not create season: ' + err.message);
      },
    });
  }

  updateSeason(update: { id: number; name: string }): void {
    this.seasonService.updateSeason(update.id, { name: update.name }).subscribe({
      next: () => {
        this.notificationService.showSuccess('Season updated successfully');
        this.loadSeasons();
      },
      error: (err: Error) => {
        this.notificationService.showError('Could not update season: ' + err.message);
      },
    });
  }

  openDeleteSeason(id: number): void {
    this.seasonToDelete = this.seasons.find((item) => item.id === id) ?? null;
  }

  closeDeleteSeasonModal(): void {
    this.seasonToDelete = null;
  }

  get selectedSeasonHasDependencies(): boolean {
    return (this.seasonToDelete?.budgets?.length ?? 0) > 0;
  }

  confirmDeleteSeason(): void {
    const season = this.seasonToDelete;
    if (!season) {
      return;
    }

    this.seasonService.deleteSeason(season.id).subscribe({
      next: (deletedSeason) => {
        const message = deletedSeason
          ? 'Season deactivated successfully'
          : 'Season deleted successfully';
        this.notificationService.showSuccess(message);
        this.closeDeleteSeasonModal();
        this.loadSeasons();
      },
      error: (err: Error) => {
        this.notificationService.showError('Could not delete season: ' + err.message);
      },
    });
  }
}
