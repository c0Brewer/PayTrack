import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { debounceTime, Subject } from 'rxjs';

import { GetTeamOptions } from '../../../types/exporter';

@Component({
  selector: 'app-team-filter-component',
  imports: [FormsModule],
  templateUrl: './team-filter-component.html',
  styleUrl: './team-filter-component.scss',
})
export class TeamFilterComponent implements OnInit {
  @Input() limitSelection: number[] = [];
  @Input() limit: number = 10;

  @Output() updateFilter = new EventEmitter<GetTeamOptions>();
  @Output() limitChange = new EventEmitter<number>();

  filterName: string = '';
  filterDescription: string = '';

  private readonly filterNameSubject = new Subject<string>();
  private readonly filterDescriptionSubject = new Subject<string>();

  ngOnInit(): void {
    this.filterNameSubject.pipe(debounceTime(400)).subscribe((value) => {
      this.filterName = value;
      this.updateFilter.emit(this.getGetTeamOptions());
    });

    this.filterDescriptionSubject.pipe(debounceTime(400)).subscribe((value) => {
      this.filterDescription = value;
      this.updateFilter.emit(this.getGetTeamOptions());
    });
  }

  getGetTeamOptions(): GetTeamOptions {
    return {
      Name: this.filterName ?? undefined,
      Description: this.filterDescription ?? undefined,
      Limit: undefined,
      Offset: undefined,
    };
  }

  onNameFilterChange(event: Event): void {
    this.filterNameSubject.next((event.target as HTMLInputElement).value);
  }

  onDescriptionFilterChange(event: Event): void {
    this.filterDescriptionSubject.next((event.target as HTMLInputElement).value);
  }

  onLimitChange(): void {
    this.limitChange.emit(this.limit);
  }
}
