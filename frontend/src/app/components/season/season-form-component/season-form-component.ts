import { Component, EventEmitter, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-season-form-component',
  imports: [FormsModule],
  templateUrl: './season-form-component.html',
  styleUrl: './season-form-component.scss',
})
export class SeasonFormComponent {
  @Output('createSeason') createSeasonEvent = new EventEmitter<string>();

  newSeasonName = '';

  createSeason(): void {
    const name = this.newSeasonName.trim();
    if (!name) {
      return;
    }

    this.createSeasonEvent.emit(name);
    this.newSeasonName = '';
  }
}
