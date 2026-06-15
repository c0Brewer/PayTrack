import { Component, Input } from '@angular/core';

import { SeasonDto } from '../../../types/exporter';

@Component({
  selector: 'app-season-list-component',
  imports: [],
  templateUrl: './season-list-component.html',
  styleUrl: './season-list-component.scss',
})
export class SeasonListComponent {
  @Input() seasons: SeasonDto[] = [];
}
