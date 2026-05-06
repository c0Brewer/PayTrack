import { Component, input } from '@angular/core';

import { UserDto } from '../../../../../types/exporter';
import { BoxComponent } from '../../../../general/boxes/box-component/box-component';

@Component({
  selector: 'app-security-settings-page',
  imports: [BoxComponent],
  templateUrl: './security-settings-page.html',
  styleUrl: './security-settings-page.scss',
})
export class SecuritySettingsPageComponent {
  user = input<UserDto | null>(null);
}
