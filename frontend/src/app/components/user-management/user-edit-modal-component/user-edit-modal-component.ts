import { ChangeDetectorRef, Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { TeamService } from '../../../services/team/team-service';
import { Role, TeamDto, UserDto } from '../../../types/exporter';
import { ModalComponent } from '../../general/modal-component/modal-component';

@Component({
  selector: 'app-user-edit-modal-component',
  imports: [FormsModule, ModalComponent],
  templateUrl: './user-edit-modal-component.html',
  styleUrl: './user-edit-modal-component.scss',
})
export class UserEditModalComponent implements OnInit {
  constructor(
    private teamService: TeamService,
    private cdr: ChangeDetectorRef,
  ) { }

  @Input() user: UserDto = {
    id: -1,
    name: '',
    email: '',
    profilePictureUrl: '',
    role: Role.REGULAR_USER,
    team: {
      id: 0,
      name: '',
      description: '',
      displayColor: '',
    },
    isActive: true,
  };

  teams: TeamDto[] = [];

  @Output() save = new EventEmitter<UserDto>();
  @Output() close = new EventEmitter<void>();

  roleOptions = [
    { value: Role.REGULAR_USER, text: 'Regular User' },
    { value: Role.TEAM_LEAD, text: 'Team Lead' },
    { value: Role.ADMIN, text: 'Admin' },
  ];

  originalUser: UserDto | null = null;

  ngOnInit(): void {
    if (!this.user.team) {
      this.user.team = { id: -1, name: 'test' };
    }

    this.teamService.getTeams().subscribe({
      next: (data) => {
        this.teams = data;
        this.cdr.markForCheck();
      },
      error: (err) => console.error(err),
    });
  }

  ngOnChanges(): void {
    if (this.user) {
      // Deep clone to avoid mutation
      this.originalUser = JSON.parse(JSON.stringify(this.user));
    }
  }

  hasUserBeenChanged(): boolean {
    if (!this.originalUser) return false;

    return (
      this.user.name !== this.originalUser.name ||
      this.user.isActive !== this.originalUser.isActive ||
      this.user.role !== this.originalUser.role ||
      this.user.team?.id !== this.originalUser.team?.id
    );
  }

  onSave(): void {
    if (!this.hasUserBeenChanged()) {
      this.onClose();
      return;
    }

    this.save.emit(this.user);
  }

  onClose(): void {
    this.close.emit();
  }
}
