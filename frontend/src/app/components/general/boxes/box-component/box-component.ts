import { CommonModule } from '@angular/common';
import { Component, input } from '@angular/core';

@Component({
  selector: 'app-box-component',
  imports: [CommonModule],
  templateUrl: './box-component.html',
  styleUrl: './box-component.scss',
})
export class BoxComponent {
  title = input('');
  subtitle = input('');
  size = input<'small' | 'medium' | 'large'>('small');
}
