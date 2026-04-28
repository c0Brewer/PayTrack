import { Component, signal } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { NavbarComponent } from './components/navbar/navbar-component/navbar-component';
import { NotificationComponent } from './components/general/notification-component/notification-component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, NavbarComponent, NotificationComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly title = signal('PayTrack');

  constructor(private readonly router: Router) {}

  protected showNavbar(): boolean {
    return !this.router.url.startsWith('/login');
  }
}
