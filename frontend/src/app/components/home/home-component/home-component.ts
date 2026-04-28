import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

import { BoxComponent } from '../../general/boxes/box-component/box-component';

@Component({
  selector: 'app-home-component',
  imports: [CommonModule, BoxComponent],
  templateUrl: './home-component.html',
  styleUrl: './home-component.scss',
})
export class HomeComponent {
  private greetings = [
    'Willkommen zurück!',
    'Schön, dich wiederzusehen!',
    '    Schön, dass du wieder da bist!',
  ];

  getGreeting(): string {
    return this.greetings[Math.floor(Math.random() * this.greetings.length)];
  }
}
