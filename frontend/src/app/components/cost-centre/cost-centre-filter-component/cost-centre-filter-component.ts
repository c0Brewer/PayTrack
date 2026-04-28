import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { debounceTime, Subject } from 'rxjs';

import { GetCostCentreOptions } from '../../../types/exporter';

@Component({
  selector: 'app-cost-centre-filter-component',
  imports: [FormsModule],
  templateUrl: './cost-centre-filter-component.html',
  styleUrl: './cost-centre-filter-component.scss',
})
export class CostCentreFilterComponent implements OnInit {
  @Input() limitSelection: number[] = [];
  @Input() limit: number = 10;

  @Output() updateFilter = new EventEmitter<GetCostCentreOptions>();
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
      this.updateFilter.emit(this.getOptions());
    });

    this.filterDescriptionSubject.pipe(debounceTime(400)).subscribe((value) => {
      this.filterDescription = value;
      this.updateFilter.emit(this.getOptions());
    });

    this.filterMinBudgetSubject.pipe(debounceTime(400)).subscribe((value) => {
      this.filterMinBudget = value;
      this.updateFilter.emit(this.getOptions());
    });

    this.filterMaxBudgetSubject.pipe(debounceTime(400)).subscribe((value) => {
      this.filterMaxBudget = value;
      this.updateFilter.emit(this.getOptions());
    });
  }

  getOptions(): GetCostCentreOptions {
    return {
      Name: this.filterName || undefined,
      Description: this.filterDescription || undefined,
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
    this.filterMinBudgetSubject.next(value ? Number(value) : undefined);
  }

  onMaxBudgetFilterChange(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.filterMaxBudgetSubject.next(value ? Number(value) : undefined);
  }

  onLimitChange(): void {
    this.limitChange.emit(this.limit);
  }
}
