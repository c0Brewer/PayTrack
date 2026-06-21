import { Component, EventEmitter, Input, OnChanges, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { NotificationService } from '../../../services/notification/notification-service';
import { UserService } from '../../../services/user/user-service';
import { Role, TeamDto, UpdateUserDto, UserDto } from '../../../types/exporter';
import { ModalComponent } from '../../general/modal-component/modal-component';

@Component({
  selector: 'app-user-edit-modal-component',
  imports: [FormsModule, ModalComponent],
  templateUrl: './user-edit-modal-component.html',
  styleUrl: './user-edit-modal-component.scss',
})
export class UserEditModalComponent implements OnChanges {
  constructor(
    private readonly userService: UserService,
    private readonly notificationService: NotificationService,
  ) {}

  @Input() user: UserDto | null = null;
  @Input() teams: TeamDto[] = [];

  @Output() saveEvent = new EventEmitter<void>();
  @Output() closeEvent = new EventEmitter<void>();

  editingUser: UserDto | null = null;

  readonly roleOptions = [
    { value: Role.REGULAR_USER, text: 'Regular User' },
    { value: Role.TEAM_LEAD, text: 'Team Lead' },
    { value: Role.ADMIN, text: 'Admin' },
  ];

  ngOnChanges(): void {
    this.editingUser = this.user
      ? {
          ...this.user,
          team: this.user.team ? { ...this.user.team } : { id: -1, name: 'No Team' },
        }
      : null;
  }

  onClose(): void {
    this.closeEvent.emit();
  }

  setActiveFromEvent(event: Event): void {
    if (!this.editingUser) return;

    this.editingUser.isActive = (event.target as HTMLInputElement).checked;
  }

  saveUser(): void {
    if (!this.editingUser) return;

    const teamId =
      this.editingUser.team?.id && this.editingUser.team.id !== -1
        ? this.editingUser.team.id
        : null;

    const updateRequest: UpdateUserDto = {
      name: this.editingUser.name,
      role: this.editingUser.role,
      isActive: this.editingUser.isActive,
      teamId,
    };

    this.userService.updateUser(this.editingUser.id, updateRequest).subscribe({
      next: () => {
        this.notificationService.showSuccess('Successfully updated user ' + this.editingUser?.name);
        this.saveEvent.emit();
      },
      error: (error: Error) => {
        this.notificationService.showError('Could not update User: ' + error);
      },
    });
  }
}
