import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { forkJoin } from 'rxjs';

import { NotificationService } from '../../../services/notification/notification-service';
import { TeamService } from '../../../services/team/team-service';
import { UserService } from '../../../services/user/user-service';
import { GetUserOptions, Role, TeamDto, UpdateUserDto, UserDto } from '../../../types/exporter';
import { StatBoxComponent } from '../../general/boxes/stat-box-component/stat-box-component';
import { PaginationComponent } from '../../general/pagination-component/pagination-component';
import { UserEditModalComponent } from '../user-edit-modal-component/user-edit-modal-component';
import { UserFilterComponent } from '../user-filter-component/user-filter-component';
import { UserListComponent } from '../user-list-component/user-list-component';

@Component({
  selector: 'app-user-management-component',
  imports: [
    StatBoxComponent,
    UserListComponent,
    UserFilterComponent,
    UserEditModalComponent,
    PaginationComponent,
  ],
  templateUrl: './user-management-component.html',
  styleUrl: './user-management-component.scss',
})
export class UserManagementComponent implements OnInit {
  constructor(
    private readonly userService: UserService,
    private readonly teamService: TeamService,
    private readonly cdr: ChangeDetectorRef,
    private readonly notificationService: NotificationService,
  ) {}

  user: UserDto[] = [];
  teams: TeamDto[] = [];

  limitSelection: number[] = [10, 25, 50];

  limit: number = this.limitSelection[0];
  page: number = 0;
  totalCount: number = 0;
  totalUserCount: number = 0;
  activeUserCount: number = 0;
  inactiveUserCount: number = 0;
  adminUserCount: number = 0;
  hasNext: boolean = false;
  hasPrev: boolean = false;

  editingUser: UserDto | null = null;
  activeStatusPendingIds = new Set<number>();

  filterOptions: GetUserOptions = {
    Name: undefined,
    Email: undefined,
    Role: undefined,
    IsActive: undefined,
    IncludeTeam: true,
    Limit: this.limit,
    Offset: this.page * this.limit,
  };

  ngOnInit(): void {
    this.loadUser();
    this.loadUserStats();
    this.loadTeams();
  }

  loadUserStats(): void {
    const baseQuery = {
      IncludeTeam: false,
      Limit: 1,
      Offset: 0,
    } satisfies GetUserOptions;

    forkJoin({
      total: this.userService.getUser(baseQuery),
      active: this.userService.getUser({ ...baseQuery, IsActive: true }),
      inactive: this.userService.getUser({ ...baseQuery, IsActive: false }),
      admins: this.userService.getUser({ ...baseQuery, Role: Role.ADMIN }),
    }).subscribe({
      next: ({ total, active, inactive, admins }) => {
          this.totalUserCount = total.totalCount ?? 0;
          this.activeUserCount = active.totalCount ?? 0;
          this.inactiveUserCount = inactive.totalCount ?? 0;
          this.adminUserCount = admins.totalCount ?? 0;
          this.cdr.markForCheck();
      },
      error: (err) => {
        this.notificationService.showError(err);
      },
    });
  }

  loadTeams(): void {
    this.teamService.getTeams({}).subscribe({
      next: (data) => {
        this.teams = data.items ?? [];
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.notificationService.showError(err);
      },
    });
  }

  loadUser(): void {
    const queryOptions: GetUserOptions = {
      Name: this.filterOptions?.Name ?? undefined,
      Email: this.filterOptions?.Email ?? undefined,
      Role: this.filterOptions?.Role ?? undefined,
      IsActive: this.filterOptions?.IsActive ?? undefined,
      IncludeTeam: true,
      Limit: this.limit,
      Offset: this.page * this.limit,
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
      this.filterOptions.Name = options.Name;
      this.filterOptions.Email = options.Email;
      this.filterOptions.Role = options.Role;
      this.filterOptions.IsActive = options.IsActive;
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
    if (this.activeStatusPendingIds.has(user.id)) {
      return;
    }

    const nextIsActive = !user.isActive;
    const updateRequest: UpdateUserDto = {
      isActive: nextIsActive,
    };

    this.setActiveStatusPending(user.id, true);

    this.userService.updateUser(user.id, updateRequest).subscribe({
      next: () => {
        this.notificationService.showSuccess(
          'Successfully changed active status of user ' + user.name,
        );
        this.user = this.user.map((currentUser) =>
          currentUser.id === user.id ? { ...currentUser, isActive: nextIsActive } : currentUser,
        );
        this.loadUserStats();
        this.setActiveStatusPending(user.id, false);
        this.cdr.markForCheck();
      },
      error: (error: Error) => {
        this.setActiveStatusPending(user.id, false);
        this.notificationService.showError('Could not update User: ' + error.message);
        this.cdr.markForCheck();
      },
    });
  }

  openEditUser(user: UserDto): void {
    this.editingUser = user;
  }

  closeEdit(): void {
    this.editingUser = null;
  }

  onUserSaved(): void {
    this.loadUser();
    this.loadUserStats();
    this.closeEdit();
  }

  private setActiveStatusPending(userId: number, isPending: boolean): void {
    const nextPendingIds = new Set(this.activeStatusPendingIds);

    if (isPending) {
      nextPendingIds.add(userId);
    } else {
      nextPendingIds.delete(userId);
    }

    this.activeStatusPendingIds = nextPendingIds;
  }
}
