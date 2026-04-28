import { CommonModule } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';

import { NotificationService } from '../../../services/notification/notification-service';
import { TeamService } from '../../../services/team/team-service';
import { TeamDto } from '../../../types/exporter';
import {NavbarComponent} from '../../navbar/navbar-component/navbar-component';
import {BoxComponent} from '../../general/boxes/box-component/box-component';

@Component({
  selector: 'app-home-component',
  imports: [CommonModule, NavbarComponent, BoxComponent],
  templateUrl: './home-component.html',
  styleUrl: './home-component.scss',
})
export class HomeComponent implements OnInit {

  private greetings = ["Willkommen zurück!", "Schön, dich wiederzusehen!", "    Schön, dass du wieder da bist!"];
  constructor(
    private readonly notificationService: NotificationService,
  ) {}

  ngOnInit(): void {
  }

  getGreeting(){
    return this.greetings[Math.floor(Math.random()*this.greetings.length)];
  }

}
