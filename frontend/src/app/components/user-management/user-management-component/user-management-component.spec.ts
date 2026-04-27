import { ChangeDetectorRef } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';

import { NotificationService } from '../../../services/notification/notification-service';
import { UserService } from '../../../services/user/user-service';
import { UserDto, Role } from '../../../types/exporter';

import { UserManagementComponent } from './user-management-component';

describe('UserManagementComponent', () => {
  let component: UserManagementComponent;
  let fixture: ComponentFixture<UserManagementComponent>;
  let userServiceMock: {
    getUser: ReturnType<typeof vi.fn>;
    updateUser: ReturnType<typeof vi.fn>;
  };
  let notificationServiceMock: {
    showError: ReturnType<typeof vi.fn>;
    showSuccess: ReturnType<typeof vi.fn>;
  };
  let cdrMock: {
    markForCheck: ReturnType<typeof vi.fn>;
  };

  const mockUsers: UserDto[] = [
    {
      id: 1,
      name: 'Alice',
      email: 'alice@test.com',
      role: Role.REGULAR_USER,
      isActive: true,
      profilePictureUrl: '',
      team: { id: 1, name: 'Team A', description: '', displayColor: '' },
    },
    {
      id: 2,
      name: 'Bob',
      email: 'bob@test.com',
      role: Role.ADMIN,
      isActive: false,
      profilePictureUrl: '',
      team: { id: 2, name: 'Team B', description: '', displayColor: '' },
    },
  ];

  beforeEach(async () => {
    userServiceMock = {
      getUser: vi
        .fn()
        .mockReturnValue(
          of({ items: mockUsers, totalCount: 2, hasNext: false, hasPrevious: false }),
        ),
      updateUser: vi.fn().mockReturnValue(of({})),
    };

    notificationServiceMock = {
      showError: vi.fn(),
      showSuccess: vi.fn(),
    };

    cdrMock = {
      markForCheck: vi.fn(),
    };

    await TestBed.configureTestingModule({
      imports: [UserManagementComponent],
      providers: [
        { provide: UserService, useValue: userServiceMock },
        { provide: NotificationService, useValue: notificationServiceMock },
        { provide: ChangeDetectorRef, useValue: cdrMock },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(UserManagementComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('ngOnInit should load users', () => {
    component.ngOnInit();
    expect(userServiceMock.getUser).toHaveBeenCalled();
    expect(component.user.length).toBe(2);
    expect(component.totalCount).toBe(2);
  });

  it('updateFilterOptions should update filter and reload users', () => {
    component.updateFilterOptions({ Name: 'Alice' });
    expect(component.page).toBe(0);
    expect(userServiceMock.getUser).toHaveBeenCalledTimes(2); // initial load + updateFilter
    expect(component.filterOptions?.Name).toBe('Alice');
  });

  it('onLimitChange should set limit and reload users', () => {
    component.onLimitChange(25);
    expect(component.limit).toBe(25);
    expect(component.page).toBe(0);
    expect(userServiceMock.getUser).toHaveBeenCalledTimes(2); // initial load + limit change
  });

  it('nextPage should increment page and load users', () => {
    component.page = 0;
    component.nextPage();
    expect(component.page).toBe(1);
    expect(userServiceMock.getUser).toHaveBeenCalledTimes(2);
  });

  it('previousPage should decrement page only if page > 0', () => {
    component.page = 1;
    component.previousPage();
    expect(component.page).toBe(0);
    component.previousPage(); // should not go below 0
    expect(component.page).toBe(0);
  });

  it('toggleActive should call updateUser and reload', () => {
    const user = { ...mockUsers[0] };
    component.toggleActive(user);
    expect(userServiceMock.updateUser).toHaveBeenCalledWith(user.id, { isActive: false });
    expect(notificationServiceMock.showSuccess).toHaveBeenCalled();
  });

  it('openEditUser should set editingUser', () => {
    const user = { ...mockUsers[0] };
    component.openEditUser(user);
    expect(component.editingUser).toEqual(user);
  });

  it('closeEdit should reset editingUser', () => {
    component.editingUser = { ...mockUsers[0] };
    component.closeEdit();
    expect(component.editingUser).toBeNull();
  });

  it('saveUser should call updateUser and reload', () => {
    const user = { ...mockUsers[0] };
    component.saveUser(user);
    expect(userServiceMock.updateUser).toHaveBeenCalled();
    expect(notificationServiceMock.showSuccess).toHaveBeenCalled();
    expect(component.editingUser).toBeNull();
  });

  it('loadUser should show error when API returns empty items', () => {
    userServiceMock.getUser.mockReturnValueOnce(of({ items: null }));
    component.loadUser();
    expect(notificationServiceMock.showError).toHaveBeenCalledWith('Error while loading Items');
  });

  it('loadUser should show error when API throws', () => {
    userServiceMock.getUser.mockReturnValueOnce(throwError(() => new Error('API error')));
    component.loadUser();
    expect(notificationServiceMock.showError).toHaveBeenCalledWith(expect.any(Error));
  });

  it('getTotalPages should calculate correctly', () => {
    component.totalCount = 23;
    component.limit = 10;
    expect(component.getTotalPages()).toBe(3);

    component.totalCount = 0;
    expect(component.getTotalPages()).toBe(1);
  });
});
