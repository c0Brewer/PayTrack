import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { of } from 'rxjs';

import { CostCentreService } from '../../../services/cost-centre/cost-centre-service';
import { NotificationService } from '../../../services/notification/notification-service';
import { PaymentRequestByTeamService } from '../../../services/payment-request-by-team/payment-request-by-team-service';
import { TeamService } from '../../../services/team/team-service';
import { UserService } from '../../../services/user/user-service';
import { PaymentRequestByTeamComponent } from './payment-request-by-team-component';

describe('PaymentRequestByTeamComponent', () => {
  let component: PaymentRequestByTeamComponent;
  let fixture: ComponentFixture<PaymentRequestByTeamComponent>;

  const mockTeamService = { getTeams: () => of({ items: [], totalCount: 0 }) };
  const mockCostCentreService = { getCostCentres: () => of({ items: [], totalCount: 0 }) };
  const mockUserService = { getUser: () => of({ items: [], totalCount: 0 }) };
  const mockNotificationService = { showSuccess: () => {}, showError: () => {} };
  const mockPaymentRequestByTeamService = { createPaymentRequestByTeam: () => of({}) };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PaymentRequestByTeamComponent, ReactiveFormsModule],
      providers: [
        { provide: TeamService, useValue: mockTeamService },
        { provide: CostCentreService, useValue: mockCostCentreService },
        { provide: UserService, useValue: mockUserService },
        { provide: NotificationService, useValue: mockNotificationService },
        { provide: PaymentRequestByTeamService, useValue: mockPaymentRequestByTeamService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(PaymentRequestByTeamComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have an invalid form when empty', () => {
    expect(component.form.invalid).toBeTrue();
  });

  it('should reject a past due date', () => {
    const yesterday = new Date();
    yesterday.setDate(yesterday.getDate() - 1);
    const isoDate = yesterday.toISOString().slice(0, 10);
    component.form.get('dueDate')!.setValue(isoDate);
    component.form.get('dueDate')!.markAsTouched();
    expect(component.form.get('dueDate')!.invalid).toBeTrue();
  });

  it('should accept a future due date', () => {
    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    const isoDate = tomorrow.toISOString().slice(0, 10);
    component.form.get('dueDate')!.setValue(isoDate);
    expect(component.form.get('dueDate')!.valid).toBeTrue();
  });

  it('selectUser should populate userId and userSearch', () => {
    const user = { id: 1, name: 'Alice', email: 'alice@example.com' } as any;
    component.selectUser(user);
    expect(component.form.get('userId')!.value).toBe(1);
    expect(component.selectedUser).toBe(user);
    expect(component.showUserDropdown).toBeFalse();
  });

  it('clearUserSelection should reset user fields', () => {
    const user = { id: 1, name: 'Alice', email: 'alice@example.com' } as any;
    component.selectUser(user);
    component.clearUserSelection();
    expect(component.form.get('userId')!.value).toBeNull();
    expect(component.selectedUser).toBeNull();
  });
});
