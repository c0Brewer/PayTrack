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
  private greetings = ['Welcome back!', 'Nice to see you again!', 'Great to have you back!'];

  greeting = this.getRandomGreeting();

  private getRandomGreeting(): string {
    return this.greetings[Math.floor(Math.random() * this.greetings.length)];
  }
}
