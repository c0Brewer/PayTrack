import { ChangeDetectorRef, Component, OnInit } from '@angular/core';

import { NotificationService } from '../../../services/notification/notification-service';
import { SeasonService } from '../../../services/season/season-service';
import { SeasonDto } from '../../../types/exporter';
import { ModalComponent } from '../../general/modal-component/modal-component';
import { PaginationComponent } from '../../general/pagination-component/pagination-component';
import { SeasonFormComponent } from '../season-form-component/season-form-component';
import { SeasonListComponent } from '../season-list-component/season-list-component';

@Component({
  selector: 'app-season-management-component',
  imports: [ModalComponent, PaginationComponent, SeasonFormComponent, SeasonListComponent],
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
  showInactiveSeasons = false;

  limitSelection: number[] = [10, 25, 50];
  limit: number = this.limitSelection[0];
  page = 0;
  totalCount = 0;
  hasNext = false;
  hasPrev = false;

  ngOnInit(): void {
    this.loadSeasons();
  }

  loadSeasons(): void {
    this.seasonService
      .getSeasonsPaginated({
        IsActive: this.showInactiveSeasons ? false : true,
        Limit: this.limit,
        Offset: this.page * this.limit,
      })
      .subscribe({
        next: (data) => {
          this.seasons = data.items ?? [];
          this.totalCount = data.totalCount;
          this.hasNext = data.hasNext ?? false;
          this.hasPrev = data.hasPrevious ?? false;
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
        this.showSeasonError('Could not create season: ', err);
      },
    });
  }

  getTotalPages(): number {
    const pageNumber = Math.ceil(this.totalCount / this.limit);
    return pageNumber > 0 ? pageNumber : 1;
  }

  onLimitChange(event: Event): void {
    const target = event.target as HTMLSelectElement;
    this.limit = Number(target.value);
    this.page = 0;
    this.loadSeasons();
  }

  nextPage(): void {
    this.page++;
    this.loadSeasons();
  }

  previousPage(): void {
    if (this.page > 0) {
      this.page--;
      this.loadSeasons();
    }
  }

  toggleInactiveSeasons(): void {
    this.showInactiveSeasons = !this.showInactiveSeasons;
    this.page = 0;
    this.loadSeasons();
  }

  updateSeason(update: { id: number; name: string }): void {
    this.seasonService.updateSeason(update.id, { name: update.name }).subscribe({
      next: () => {
        this.notificationService.showSuccess('Season updated successfully');
        this.loadSeasons();
      },
      error: (err: Error) => {
        this.showSeasonError('Could not update season: ', err);
      },
    });
  }

  reactivateSeason(id: number): void {
    this.seasonService.updateSeason(id, { isActive: true }).subscribe({
      next: () => {
        this.notificationService.showSuccess('Season reactivated successfully');
        this.loadSeasons();
      },
      error: (err: Error) => {
        this.showSeasonError('Could not reactivate season: ', err);
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

  private showSeasonError(prefix: string, err: Error): void {
    if (err.message === 'season name already taken') {
      this.notificationService.showError('season name already taken');
      return;
    }

    this.notificationService.showError(prefix + err.message);
  }
}
