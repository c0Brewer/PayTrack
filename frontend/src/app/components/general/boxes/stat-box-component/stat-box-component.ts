import { Component, input } from '@angular/core';

@Component({
  selector: 'app-stat-box-component',
  imports: [],
  templateUrl: './stat-box-component.html',
  styleUrl: './stat-box-component.scss',
})
export class StatBoxComponent {
  header = input('');
  content = input<string | number>('');
  icon = input('');
  size = input<'default' | 'small'>('default');
  iconColor = input<string | null>(null);
  iconBackgroundColor = input<string | null>(null);
}
