import { ChangeDetectorRef } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';

import { NotificationService } from '../../../services/notification/notification-service';
import { TeamService } from '../../../services/team/team-service';
import { Role, TeamDto, UserDto } from '../../../types/exporter';

import { UserEditModalComponent } from './user-edit-modal-component';

describe('UserEditModalComponent', () => {
  let component: UserEditModalComponent;
  let fixture: ComponentFixture<UserEditModalComponent>;
  let teamServiceMock: Partial<TeamService>;
  let notificationServiceMock: Partial<NotificationService>;
  let cdrMock: Partial<ChangeDetectorRef>;

  const mockTeams: TeamDto[] = [
    { id: 1, name: 'Team 1', description: '', displayColor: '' },
    { id: 2, name: 'Team 2', description: '', displayColor: '' },
  ];

  beforeEach(async () => {
    teamServiceMock = {
      getTeams: vi.fn().mockReturnValue(of(mockTeams)),
    };
    notificationServiceMock = {
      showError: vi.fn(),
    };
    cdrMock = {
      markForCheck: vi.fn(),
    };

    await TestBed.configureTestingModule({
      imports: [UserEditModalComponent],
      providers: [
        { provide: TeamService, useValue: teamServiceMock },
        { provide: NotificationService, useValue: notificationServiceMock },
        { provide: ChangeDetectorRef, useValue: cdrMock },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(UserEditModalComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('ngOnInit should call showError if teamService fails', () => {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    (teamServiceMock.getTeams as any).mockReturnValue(throwError(() => 'Error'));
    component.ngOnInit();
    expect(notificationServiceMock.showError).toHaveBeenCalledWith('Error');
  });

  it('ngOnChanges should deep clone user to originalUser', () => {
    const user: UserDto = {
      id: 1,
      name: 'Alice',
      email: 'a@test.com',
      profilePictureUrl: '',
      role: Role.REGULAR_USER,
      isActive: true,
      team: { id: 1, name: 'Team', description: '', displayColor: '' },
      bankAccounts: [],
    };
    component.user = user;
    component.ngOnChanges();

    expect(component.originalUser).not.toBeNull();
    expect(component.originalUser).toEqual(user);
    expect(component.originalUser).not.toBe(user); // ensure deep clone
  });

  it('hasUserBeenChanged should detect changes', () => {
    component.user.name = 'Alice';
    component.originalUser = { ...component.user, name: 'Bob' };
    expect(component.hasUserBeenChanged()).toBe(true);

    component.originalUser.name = 'Alice';
    expect(component.hasUserBeenChanged()).toBe(false);
  });

  it('onSave should emit saveEvent if user changed', () => {
    component.user.name = 'Alice';
    component.originalUser = { ...component.user, name: 'Bob' };
    const spy = vi.spyOn(component.saveEvent, 'emit');

    component.onSave();
    expect(spy).toHaveBeenCalledWith(component.user);
  });

  it('onSave should call onClose if user unchanged', () => {
    component.user.name = 'Alice';
    component.originalUser = { ...component.user };
    const spyClose = vi.spyOn(component, 'onClose');

    component.onSave();
    expect(spyClose).toHaveBeenCalled();
  });

  it('onClose should emit closeEvent', () => {
    const spy = vi.spyOn(component.closeEvent, 'emit');
    component.onClose();
    expect(spy).toHaveBeenCalled();
  });
});
