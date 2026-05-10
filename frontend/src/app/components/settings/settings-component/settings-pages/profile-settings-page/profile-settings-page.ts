import { Component, input } from '@angular/core';

import { UserDto } from '../../../../../types/exporter';
import { BoxComponent } from '../../../../general/boxes/box-component/box-component';

@Component({
  selector: 'app-profile-settings-page',
  imports: [BoxComponent],
  templateUrl: './profile-settings-page.html',
  styleUrl: './profile-settings-page.scss',
})
export class ProfileSettingsPageComponent {
  user = input<UserDto | null>(null);
}
