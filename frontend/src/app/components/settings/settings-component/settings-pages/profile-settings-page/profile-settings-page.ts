import { Component, input } from '@angular/core';

import { Role, UserDto } from '../../../../../types/exporter';
import { BoxComponent } from '../../../../general/boxes/box-component/box-component';

@Component({
  selector: 'app-profile-settings-page',
  imports: [BoxComponent],
  templateUrl: './profile-settings-page.html',
  styleUrl: './profile-settings-page.scss',
})
export class ProfileSettingsPageComponent {
  user = input<UserDto | null>(null);

  protected roleToText(role: Role | undefined): string {
    switch (role) {
      case Role.REGULAR_USER:
        return 'Regular User';
      case Role.TEAM_LEAD:
        return 'Team Lead';
      case Role.ADMIN:
        return 'Admin';
      default:
        return 'Unknown';
    }
  }

  protected getBankSetupStatus(user: UserDto | null): string {
    if (!user) {
      return 'Unknown';
    }

    if (user.hasBankInformation) {
      return `${user.bankAccounts?.length ?? 0} account(s) configured`;
    }

    return user.bankInformationSkipped ? 'Skipped during onboarding' : 'Action required';
  }
}
