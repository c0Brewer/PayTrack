import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';

import { NotificationService } from '../../../services/notification/notification-service';
import { BoxComponent } from '../../general/boxes/box-component/box-component';
import { NavbarComponent } from '../../navbar/navbar-component/navbar-component';

@Component({
  selector: 'app-home-component',
  imports: [CommonModule, NavbarComponent, BoxComponent],
  templateUrl: './home-component.html',
  styleUrl: './home-component.scss',
})
export class HomeComponent implements OnInit {
  private greetings = [
    'Willkommen zurück!',
    'Schön, dich wiederzusehen!',
    '    Schön, dass du wieder da bist!',
  ];
  constructor(private readonly notificationService: NotificationService) {}

  getGreeting(): string {
    return this.greetings[Math.floor(Math.random() * this.greetings.length)];
  }
}
