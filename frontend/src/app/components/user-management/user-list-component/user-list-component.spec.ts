import { ComponentFixture, TestBed } from '@angular/core/testing';
import { vi } from 'vitest';

import { Role, UserDto } from '../../../types/exporter';

import { UserListComponent } from './user-list-component';

describe('UserListComponent', () => {
  let component: UserListComponent;
  let fixture: ComponentFixture<UserListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UserListComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(UserListComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create the component', () => {
    expect(component).toBeTruthy();
  });

  it('should emit toggleActive when onToggleActive is called', () => {
    const mockUser: UserDto = {
      id: 1,
      name: 'Test User',
      email: 'test@example.com',
      isActive: true,
      team: { id: 1, name: 'Team A' },
      role: Role.REGULAR_USER,
      profilePictureUrl: 'pic.png',
      bankAccounts: [],
      bankInformationSkipped: true,
      hasBankInformation: true,
    };

    const spy = vi.spyOn(component.toggleActive, 'emit');

    component.onToggleActive(mockUser);

    expect(spy).toHaveBeenCalledOnce();
    expect(spy).toHaveBeenCalledWith(mockUser);
  });

  it('should emit openEditUser when onOpenEditUser is called', () => {
    const mockUser: UserDto = {
      id: 2,
      name: 'Jane Doe',
      email: 'jane@example.com',
      isActive: false,
      team: { id: 2, name: 'Team B' },
      role: Role.TEAM_LEAD,
      profilePictureUrl: 'pic2.png',
      bankAccounts: [],
      bankInformationSkipped: true,
      hasBankInformation: true,
    };

    const spy = vi.spyOn(component.openEditUser, 'emit');

    component.onOpenEditUser(mockUser);

    expect(spy).toHaveBeenCalledOnce();
    expect(spy).toHaveBeenCalledWith(mockUser);
  });

  it('should convert roles to readable text', () => {
    expect(component.roleToText(Role.REGULAR_USER)).toBe('Regular User');
    expect(component.roleToText(Role.TEAM_LEAD)).toBe('Team Lead');
    expect(component.roleToText(Role.ADMIN)).toBe('Admin');
    expect(component.roleToText(-1 as Role)).toBe('Unknown'); // fallback case
  });

  it('should accept @Input user array', () => {
    const users: UserDto[] = [
      {
        id: 1,
        name: 'Alice',
        email: 'a@a.com',
        isActive: true,
        team: { id: 1, name: 'Team 1' },
        role: Role.ADMIN,
        profilePictureUrl: '',
        bankAccounts: [],
        bankInformationSkipped: true,
        hasBankInformation: true,
      },
      {
        id: 2,
        name: 'Bob',
        email: 'b@b.com',
        isActive: false,
        team: { id: 2, name: 'Team 2' },
        role: Role.TEAM_LEAD,
        profilePictureUrl: '',
        bankAccounts: [],
        bankInformationSkipped: true,
        hasBankInformation: true,
      },
    ];

    component.user = users;
    expect(component.user).toEqual(users);
  });
});
