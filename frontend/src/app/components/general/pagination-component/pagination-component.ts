import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-pagination-component',
  imports: [],
  templateUrl: './pagination-component.html',
  styleUrl: './pagination-component.scss',
})
export class PaginationComponent {
  @Input() hasNext = false;
  @Input() hasPrev = false;
  @Input() currentPage: number = 1;
  @Input() maxPage: number = 1;

  @Output() next = new EventEmitter<void>();
  @Output() prev = new EventEmitter<void>();

  onNext(): void {
    this.next.emit();
  }

  onPrev(): void {
    this.prev.emit();
  }
}
