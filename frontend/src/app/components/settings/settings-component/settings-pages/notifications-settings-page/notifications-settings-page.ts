import { Component, input } from '@angular/core';

import { UserDto } from '../../../../../types/exporter';
import { BoxComponent } from '../../../../general/boxes/box-component/box-component';

@Component({
  selector: 'app-notifications-settings-page',
  imports: [BoxComponent],
  templateUrl: './notifications-settings-page.html',
  styleUrl: './notifications-settings-page.scss',
})
export class NotificationsSettingsPageComponent {
  user = input<UserDto | null>(null);
}
