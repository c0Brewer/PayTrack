import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-detail-component',
  imports: [],
  templateUrl: './detail-component.html',
  styleUrl: './detail-component.scss',
})
export class DetailComponent {
  @Input() title = '';
  @Input() subtitle: string | null = null;
  @Input() eyebrow: string | null = null;
  @Input() icon: string | null = null;
  @Input() backLabel: string | null = null;
  @Input() statusLabel: string | null = null;
  @Input() statusVariant: 'active' | 'inactive' | 'neutral' = 'neutral';

  @Output() backClick = new EventEmitter<void>();

  onBackClick(): void {
    this.backClick.emit();
  }
}
