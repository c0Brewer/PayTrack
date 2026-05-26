import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';

import { NotificationService } from '../../../services/notification/notification-service';
import { UserService } from '../../../services/user/user-service';
import { Role, UserDto } from '../../../types/exporter';

import { UserEditModalComponent } from './user-edit-modal-component';

describe('UserEditModalComponent', () => {
  let component: UserEditModalComponent;
  let fixture: ComponentFixture<UserEditModalComponent>;
  let userServiceMock: {
    updateUser: ReturnType<typeof vi.fn>;
  };
  let notificationServiceMock: {
    showError: ReturnType<typeof vi.fn>;
    showSuccess: ReturnType<typeof vi.fn>;
  };

  const mockUser: UserDto = {
    id: 1,
    name: 'Alice',
    email: 'alice@test.com',
    role: Role.REGULAR_USER,
    isActive: true,
    profilePictureUrl: '',
    team: { id: 1, name: 'Team A', description: '', displayColor: '' },
    bankAccounts: [],
    bankInformationSkipped: true,
    hasBankInformation: true,
  };

  beforeEach(async () => {
    userServiceMock = {
      updateUser: vi.fn().mockReturnValue(of({})),
    };

    notificationServiceMock = {
      showError: vi.fn(),
      showSuccess: vi.fn(),
    };

    await TestBed.configureTestingModule({
      imports: [UserEditModalComponent],
      providers: [
        { provide: UserService, useValue: userServiceMock },
        { provide: NotificationService, useValue: notificationServiceMock },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(UserEditModalComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('ngOnChanges should clone the input user for editing', () => {
    component.user = mockUser;

    component.ngOnChanges();

    expect(component.editingUser).toEqual(mockUser);
    expect(component.editingUser).not.toBe(mockUser);
    expect(component.editingUser?.team).not.toBe(mockUser.team);
  });

  it('ngOnChanges should add a no-team fallback for users without a team', () => {
    component.user = { ...mockUser, team: undefined } as unknown as UserDto;

    component.ngOnChanges();

    expect(component.editingUser?.team).toEqual({ id: -1, name: 'No Team' });
  });

  it('saveUser should update the user and emit saveEvent', () => {
    const emitSpy = vi.spyOn(component.saveEvent, 'emit');
    component.user = mockUser;
    component.ngOnChanges();

    component.saveUser();

    expect(userServiceMock.updateUser).toHaveBeenCalledWith(mockUser.id, {
      name: mockUser.name,
      role: mockUser.role,
      isActive: mockUser.isActive,
      teamId: mockUser.team?.id,
    });
    expect(notificationServiceMock.showSuccess).toHaveBeenCalledWith(
      'Successfully updated user ' + mockUser.name,
    );
    expect(emitSpy).toHaveBeenCalled();
  });

  it('saveUser should send null teamId when no team is selected', () => {
    component.user = { ...mockUser, team: undefined } as unknown as UserDto;
    component.ngOnChanges();

    component.saveUser();

    expect(userServiceMock.updateUser).toHaveBeenCalledWith(
      mockUser.id,
      expect.objectContaining({ teamId: null }),
    );
  });

  it('saveUser should show an error when update fails', () => {
    const error = new Error('API error');
    const emitSpy = vi.spyOn(component.saveEvent, 'emit');
    userServiceMock.updateUser.mockReturnValueOnce(throwError(() => error));
    component.user = mockUser;
    component.ngOnChanges();

    component.saveUser();

    expect(notificationServiceMock.showError).toHaveBeenCalledWith(
      'Could not update User: ' + error,
    );
    expect(emitSpy).not.toHaveBeenCalled();
  });

  it('onClose should emit closeEvent', () => {
    const emitSpy = vi.spyOn(component.closeEvent, 'emit');

    component.onClose();

    expect(emitSpy).toHaveBeenCalled();
  });
});
