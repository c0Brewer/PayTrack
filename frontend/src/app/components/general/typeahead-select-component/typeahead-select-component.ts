import { CommonModule } from '@angular/common';
import {
  Component,
  ElementRef,
  EventEmitter,
  HostListener,
  Input,
  OnDestroy,
  OnInit,
  Output,
} from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { Subject, filter, takeUntil } from 'rxjs';

export interface TypeaheadItem {
  id: number | string;
  primaryText: string;
  secondaryText?: string;
}

@Component({
  selector: 'app-typeahead-select',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './typeahead-select-component.html',
  styleUrl: './typeahead-select-component.scss',
})
export class TypeaheadSelectComponent implements OnInit, OnDestroy {
  @Input({ required: true }) items!: TypeaheadItem[];
  @Input() placeholder = 'Search…';
  @Input() minChars = 1;
  @Input() isInvalid = false;

  @Output() itemSelected = new EventEmitter<TypeaheadItem>();
  @Output() cleared = new EventEmitter<void>();

  readonly searchControl = new FormControl('');
  results: TypeaheadItem[] = [];
  showDropdown = false;
  selectedItem: TypeaheadItem | null = null;

  private readonly destroy$ = new Subject<void>();

  constructor(private readonly el: ElementRef) {}

  ngOnInit(): void {
    this.searchControl.valueChanges
      .pipe(
        takeUntil(this.destroy$),
        filter((term) => typeof term === 'string'),
      )
      .subscribe((term) => {
        const q = (term ?? '').trim().toLowerCase();
        if (q.length < this.minChars) {
          this.results = [];
          this.showDropdown = false;
          return;
        }
        this.results = this.items.filter(
          (item) =>
            item.primaryText.toLowerCase().includes(q) ||
            (item.secondaryText?.toLowerCase().includes(q) ?? false),
        );
        this.showDropdown = this.results.length > 0;
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  @HostListener('document:mousedown', ['$event'])
  onDocumentMousedown(event: MouseEvent): void {
    if (!this.el.nativeElement.contains(event.target)) {
      this.showDropdown = false;
    }
  }

  onFocus(): void {
    if (this.results.length > 0 && !this.selectedItem) {
      this.showDropdown = true;
    }
  }

  select(item: TypeaheadItem): void {
    this.selectedItem = item;
    const display = item.secondaryText
      ? `${item.primaryText} (${item.secondaryText})`
      : item.primaryText;
    this.searchControl.setValue(display, { emitEvent: false });
    this.showDropdown = false;
    this.itemSelected.emit(item);
  }

  clear(): void {
    this.selectedItem = null;
    this.searchControl.setValue('', { emitEvent: false });
    this.results = [];
    this.showDropdown = false;
    this.cleared.emit();
  }

  public reset(): void {
    this.clear();
  }
}
