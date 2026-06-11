import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { SeasonDto } from '../../../types/exporter';

@Component({
  selector: 'app-season-list-component',
  imports: [FormsModule],
  templateUrl: './season-list-component.html',
  styleUrl: './season-list-component.scss',
})
export class SeasonListComponent {
  @Input() seasons: SeasonDto[] = [];
  @Output() updateSeason = new EventEmitter<{ id: number; name: string }>();
  @Output() deleteSeason = new EventEmitter<number>();

  editingSeasonId: number | null = null;
  editedSeasonName = '';

  get visibleSeasons(): SeasonDto[] {
    return this.seasons.filter((season) => season.isActive !== false);
  }

  startEdit(season: SeasonDto): void {
    this.editingSeasonId = season.id;
    this.editedSeasonName = season.name;
  }

  cancelEdit(): void {
    this.editingSeasonId = null;
    this.editedSeasonName = '';
  }

  submitEdit(season: SeasonDto): void {
    const name = this.editedSeasonName.trim();
    if (!name) {
      return;
    }

    if (name !== season.name) {
      this.updateSeason.emit({ id: season.id, name });
    }

    this.cancelEdit();
  }

  requestDelete(season: SeasonDto): void {
    this.deleteSeason.emit(season.id);
  }
}
