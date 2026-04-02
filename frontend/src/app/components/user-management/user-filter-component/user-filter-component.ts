import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { debounceTime, Subject } from 'rxjs';

import { GetUserOptions, Role } from '../../../types/exporter';

@Component({
  selector: 'app-user-filter-component',
  imports: [FormsModule],
  templateUrl: './user-filter-component.html',
  styleUrl: './user-filter-component.scss',
})
export class UserFilterComponent implements OnInit {
  @Input() limitSelection: number[] = [];
  @Input() limit: number = 10;

  @Output() updateFilter = new EventEmitter<GetUserOptions>();
  @Output() limitChange = new EventEmitter<number>();

  // Explicit filters
  filterName: string = '';
  filterEmail: string = '';
  filterRole: Role | undefined = undefined;
  filterIsActive: boolean | undefined = undefined;

  // Debounce subjects
  private readonly filterNameSubject = new Subject<string>();
  private readonly filterEmailSubject = new Subject<string>();
  private readonly filterRoleSubject = new Subject<Role | undefined>();
  private readonly filterIsActiveSubject = new Subject<boolean | undefined>();

  ngOnInit(): void {
    // Setup debounce
    this.filterNameSubject.pipe(debounceTime(400)).subscribe((value) => {
      this.filterName = value;
      this.updateFilter.emit(this.getGetUserOptions());
    });

    this.filterEmailSubject.pipe(debounceTime(400)).subscribe((value) => {
      this.filterEmail = value;
      this.updateFilter.emit(this.getGetUserOptions());
    });

    this.filterRoleSubject.pipe(debounceTime(100)).subscribe((value) => {
      this.filterRole = value ?? undefined;
      this.updateFilter.emit(this.getGetUserOptions());
    });

    this.filterIsActiveSubject.pipe(debounceTime(100)).subscribe((value) => {
      this.filterIsActive = value ?? undefined;
      this.updateFilter.emit(this.getGetUserOptions());
    });
  }

  getGetUserOptions(): GetUserOptions {
    return {
      name: this.filterName ?? undefined,
      email: this.filterEmail ?? undefined,
      role: this.filterRole ?? undefined,
      isActive: this.filterIsActive ?? undefined,
      includeTeam: undefined,
      limit: undefined,
      offset: undefined,
    };
  }

  // Filter handlers
  onNameFilterChange(event: Event): void {
    this.filterNameSubject.next((event.target as HTMLInputElement).value);
  }

  onEmailFilterChange(event: Event): void {
    this.filterEmailSubject.next((event.target as HTMLInputElement).value);
  }

  onRoleFilterChange(event: Event): void {
    const value = (event.target as HTMLSelectElement).value;
    this.filterRoleSubject.next(value ? Number(value) : undefined);
  }

  onIsActiveFilterChange(): void {
    this.filterIsActiveSubject.next(this.filterIsActive);
  }

  onLimitChange(): void {
    this.limitChange.emit(this.limit);
  }
}
