import { Component, EventEmitter, Input, Output } from '@angular/core';

import { Role, UserDto } from '../../../types/exporter';

@Component({
  selector: 'app-user-list-component',
  imports: [],
  templateUrl: './user-list-component.html',
  styleUrl: './user-list-component.scss',
})
export class UserListComponent {
  @Input() user: UserDto[] = [];

  @Output() toggleActive = new EventEmitter<UserDto>();
  @Output() openEditUser = new EventEmitter<UserDto>();

  // Role Definitions
  regular_user = Role.REGULAR_USER;
  team_lead = Role.TEAM_LEAD;
  admin = Role.ADMIN;

  onToggleActive(user: UserDto): void {
    this.toggleActive.emit(user);
  }

  onOpenEditUser(user: UserDto): void {
    this.openEditUser.emit(user);
  }

  roleToText(role: Role): string {
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
}
