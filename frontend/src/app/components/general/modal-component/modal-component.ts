import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-modal-component',
  imports: [],
  templateUrl: './modal-component.html',
  styleUrl: './modal-component.scss',
})
export class ModalComponent {
  @Input() visible = true;
  @Input() title: string = '';
  @Input() info: string = '';

  @Output() closeEvent = new EventEmitter<void>();

  onClose(): void {
    this.closeEvent.emit();
  }
}
