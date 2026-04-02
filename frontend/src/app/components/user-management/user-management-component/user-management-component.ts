import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { NotificationService } from '../../../services/notification/notification-service';
import { UserService } from '../../../services/user/user-service';
import { UserDto, GetUserOptions, UpdateUserDto } from '../../../types/exporter';
import { PaginationComponent } from '../../general/pagination-component/pagination-component';
import { UserEditModalComponent } from '../user-edit-modal-component/user-edit-modal-component';
import { UserFilterComponent } from '../user-filter-component/user-filter-component';
import { UserListComponent } from '../user-list-component/user-list-component';

@Component({
  selector: 'app-user-management-component',
  imports: [
    FormsModule,
    UserEditModalComponent,
    PaginationComponent,
    UserListComponent,
    UserFilterComponent,
  ],
  templateUrl: './user-management-component.html',
  styleUrl: './user-management-component.scss',
})
export class UserManagementComponent implements OnInit {
  constructor(
    private readonly userService: UserService,
    private readonly cdr: ChangeDetectorRef,
    private readonly notificationService: NotificationService,
  ) {}

  user: UserDto[] = [];

  limitSelection: number[] = [10, 25, 50];

  limit: number = this.limitSelection[0];
  page: number = 0;
  totalCount: number = 0;
  hasNext: boolean = false;
  hasPrev: boolean = false;

  editingUser: UserDto | null = null;

  filterOptions: GetUserOptions = {
    name: undefined,
    email: undefined,
    role: undefined,
    isActive: undefined,
    includeTeam: true,
    limit: this.limit,
    offset: this.page * this.limit,
  };

  ngOnInit(): void {
    this.loadUser();
  }

  loadUser(): void {
    const queryOptions: GetUserOptions = {
      name: this.filterOptions?.name ?? undefined,
      email: this.filterOptions?.email ?? undefined,
      role: this.filterOptions?.role ?? undefined,
      isActive: this.filterOptions?.isActive ?? undefined,
      includeTeam: true,
      limit: this.limit,
      offset: this.page * this.limit,
    };

    this.userService.getUser(queryOptions).subscribe({
      next: (data) => {
        if (data?.items) {
          this.user = data.items;
          this.totalCount = data.totalCount;
          this.hasNext = data.hasNext ?? false;
          this.hasPrev = data.hasPrevious ?? false;

          // Mark for refresh
          this.cdr.markForCheck();
        } else {
          this.notificationService.showError('Error while loading Items');
        }
      },
      error: (err) => {
        this.notificationService.showError(err);
      },
    });
  }

  updateFilterOptions(options: GetUserOptions): void {
    if (this.filterOptions && options) {
      this.filterOptions.name = options.name;
      this.filterOptions.email = options.email;
      this.filterOptions.role = options.role;
      this.filterOptions.isActive = options.isActive;
      this.page = 0;
      this.loadUser();
    }
  }

  getTotalPages(): number {
    const pageNumber = Math.ceil(this.totalCount / this.limit);
    return pageNumber > 0 ? pageNumber : 1;
  }

  onLimitChange(limit: number): void {
    this.limit = limit;
    this.page = 0;
    this.loadUser();
  }

  nextPage(): void {
    this.page++;
    this.loadUser();
  }

  previousPage(): void {
    if (this.page > 0) {
      this.page--;
      this.loadUser();
    }
  }

  toggleActive(user: UserDto): void {
    const updateRequest: UpdateUserDto = {
      isActive: !user.isActive,
    };

    this.userService.updateUser(user.id, updateRequest).subscribe({
      next: () => {
        this.notificationService.showSuccess(
          'Successfully changed active status of user ' + user.name,
        );
        this.loadUser();
        this.closeEdit();
      },
      error: (error: Error) => {
        this.notificationService.showError('Could not update User: ' + error);
      },
    });

    user.isActive = !user.isActive;
  }

  openEditUser(user: UserDto): void {
    this.editingUser = { ...user };
  }

  closeEdit(): void {
    this.editingUser = null;
  }

  saveUser(user: UserDto): void {
    if (!user) return;

    // Only set teamid if explicitly set
    const teamId = user.team?.id && user.team.id != -1 ? user.team.id : null;

    const updateRequest: UpdateUserDto = {
      name: user.name,
      role: user.role,
      isActive: user.isActive,
      teamId: teamId,
    };

    this.userService.updateUser(user.id, updateRequest).subscribe({
      next: () => {
        this.notificationService.showSuccess('Successfully updated user ' + user.name);
        this.loadUser();
        this.closeEdit();
      },
      error: (error: Error) => {
        this.notificationService.showError('Could not update User: ' + error);
      },
    });
  }
}
