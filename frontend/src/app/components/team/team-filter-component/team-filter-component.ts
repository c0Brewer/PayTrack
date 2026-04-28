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
  filterMinBudget: number | undefined = undefined;
  filterMaxBudget: number | undefined = undefined;

  private readonly filterNameSubject = new Subject<string>();
  private readonly filterDescriptionSubject = new Subject<string>();
  private readonly filterMinBudgetSubject = new Subject<number | undefined>();
  private readonly filterMaxBudgetSubject = new Subject<number | undefined>();

  ngOnInit(): void {
    this.filterNameSubject.pipe(debounceTime(400)).subscribe((value) => {
      this.filterName = value;
      this.updateFilter.emit(this.getGetTeamOptions());
    });

    this.filterDescriptionSubject.pipe(debounceTime(400)).subscribe((value) => {
      this.filterDescription = value;
      this.updateFilter.emit(this.getGetTeamOptions());
    });

    this.filterMinBudgetSubject.pipe(debounceTime(100)).subscribe((value) => {
      this.filterMinBudget = value ?? undefined;
      this.updateFilter.emit(this.getGetTeamOptions());
    });

    this.filterMaxBudgetSubject.pipe(debounceTime(100)).subscribe((value) => {
      this.filterMaxBudget = value ?? undefined;
      this.updateFilter.emit(this.getGetTeamOptions());
    });
  }

  getGetTeamOptions(): GetTeamOptions {
    return {
      Name: this.filterName ?? undefined,
      Description: this.filterDescription ?? undefined,
      MinBudget: this.filterMinBudget ?? undefined,
      MaxBudget: this.filterMaxBudget ?? undefined,
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

  onMinBudgetFilterChange(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.filterMinBudgetSubject.next(value === '' ? undefined : Number(value));
  }

  onMaxBudgetFilterChange(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.filterMaxBudgetSubject.next(value === '' ? undefined : Number(value));
  }

  onLimitChange(): void {
    this.limitChange.emit(this.limit);
  }
}
