import { ChangeDetectorRef } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';

import { AuthService } from '../../../../../services/auth/auth-service';
import { NotificationService } from '../../../../../services/notification/notification-service';
import { PaymentRequestByTeamService } from '../../../../../services/payment-request-by-team/payment-request-by-team-service';
import { TeamService } from '../../../../../services/team/team-service';
import { UserService } from '../../../../../services/user/user-service';
import { PaymentRequestByTeamDto, TransactionStatus, UserDto } from '../../../../../types/exporter';

import { TeamRequestTeamOverviewComponent } from './team-overview-component';

describe('TeamRequestTeamOverviewComponent', () => {
  let component: TeamRequestTeamOverviewComponent;
  let fixture: ComponentFixture<TeamRequestTeamOverviewComponent>;

  const mockUser = { id: 42, role: 0 } as UserDto;

  const paymentServiceMock = {
    getPaymentRequestsByTeam: vi.fn(),
  };

  const authServiceMock = {
    getCurrentUser: vi.fn().mockReturnValue(of(mockUser)),
  };

  const notificationMock = {
    showError: vi.fn(),
  };

  const routerMock = {
    navigate: vi.fn(),
  };

  const cdrMock = {
    markForCheck: vi.fn(),
  };

  const teamServiceMock = {
    getTeams: vi.fn().mockReturnValue(of({ items: [], totalCount: 0 })),
  };

  const userServiceMock = {
    getUser: vi.fn().mockReturnValue(of({ items: [], totalCount: 0 })),
  };

  beforeEach(async () => {
    vi.clearAllMocks();

    await TestBed.configureTestingModule({
      imports: [TeamRequestTeamOverviewComponent],
      providers: [
        { provide: PaymentRequestByTeamService, useValue: paymentServiceMock },
        { provide: AuthService, useValue: authServiceMock },
        { provide: NotificationService, useValue: notificationMock },
        { provide: Router, useValue: routerMock },
        { provide: ChangeDetectorRef, useValue: cdrMock },
        { provide: TeamService, useValue: teamServiceMock },
        { provide: UserService, useValue: userServiceMock },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(TeamRequestTeamOverviewComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    paymentServiceMock.getPaymentRequestsByTeam.mockReturnValue(
      of({ items: [], totalCount: 0, hasNext: false, hasPrevious: false }),
    );
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should fetch the current user and then load requests on init', () => {
    paymentServiceMock.getPaymentRequestsByTeam.mockReturnValue(
      of({ items: [], totalCount: 0, hasNext: false, hasPrevious: false }),
    );
    component.ngOnInit();
    expect(authServiceMock.getCurrentUser).toHaveBeenCalled();
    expect(paymentServiceMock.getPaymentRequestsByTeam).toHaveBeenCalled();
  });

  it('should include the current user id in every query', () => {
    paymentServiceMock.getPaymentRequestsByTeam.mockReturnValue(
      of({ items: [], totalCount: 0, hasNext: false, hasPrevious: false }),
    );
    component.ngOnInit();
    expect(paymentServiceMock.getPaymentRequestsByTeam).toHaveBeenCalledWith(
      expect.objectContaining({ UserId: 42 }),
    );
  });

  it('should set requests and pagination state on successful load', () => {
    const items = [
      { id: 1, amount: 100 },
      { id: 2, amount: 200 },
    ] as PaymentRequestByTeamDto[];
    paymentServiceMock.getPaymentRequestsByTeam.mockReturnValue(
      of({ items, totalCount: 2, hasNext: true, hasPrevious: false }),
    );

    component.loadRequests();

    expect(component.requests).toEqual(items);
    expect(component.totalCount).toBe(2);
    expect(component.hasNext).toBe(true);
    expect(component.hasPrev).toBe(false);
  });

  it('should load stat requests when requests are loaded', () => {
    const pageItems = [{ id: 1, amount: 100, status: TransactionStatus.Submitted }];
    const statItems = [
      { id: 1, amount: 100, status: TransactionStatus.Submitted },
      { id: 2, amount: 200, status: TransactionStatus.Paid },
    ] as PaymentRequestByTeamDto[];
    paymentServiceMock.getPaymentRequestsByTeam
      .mockReturnValueOnce(
        of({ items: pageItems, totalCount: 2, hasNext: false, hasPrevious: false }),
      )
      .mockReturnValueOnce(
        of({ items: statItems, totalCount: 2, hasNext: false, hasPrevious: false }),
      );

    component.loadRequests();

    expect(component.statRequests).toEqual(statItems);
    expect(paymentServiceMock.getPaymentRequestsByTeam).toHaveBeenLastCalledWith(
      expect.objectContaining({ Limit: 2, Offset: 0 }),
    );
  });

  it('should calculate stat box values from stat requests', () => {
    component.statRequests = [
      { id: 1, amount: 100, status: TransactionStatus.Submitted },
      { id: 2, amount: 200, status: TransactionStatus.Paid },
      { id: 3, amount: 50, status: TransactionStatus.Submitted },
    ] as PaymentRequestByTeamDto[];

    expect(component.getTotalAmount()).toBe(350);
    expect(component.getSubmittedRequestCount()).toBe(2);
    expect(component.getPaidRequestCount()).toBe(1);
  });

  it('should show error on API failure', () => {
    paymentServiceMock.getPaymentRequestsByTeam.mockReturnValue(throwError(() => 'API error'));

    component.loadRequests();

    expect(notificationMock.showError).toHaveBeenCalledWith('API error');
  });

  it('should reset page to 0 and reload when filter options are updated', () => {
    paymentServiceMock.getPaymentRequestsByTeam.mockReturnValue(
      of({ items: [], totalCount: 0, hasNext: false, hasPrevious: false }),
    );
    component.page = 3;

    component.updateFilterOptions({ Status: 2 as 0 | 1 | 2 | 3 | 4 });

    expect(component.page).toBe(0);
    expect(paymentServiceMock.getPaymentRequestsByTeam).toHaveBeenCalled();
  });

  it('should update limit, reset page, and reload on onUpdateLimit', () => {
    paymentServiceMock.getPaymentRequestsByTeam.mockReturnValue(
      of({ items: [], totalCount: 0, hasNext: false, hasPrevious: false }),
    );
    component.page = 2;

    component.onUpdateLimit(25);

    expect(component.limit).toBe(25);
    expect(component.page).toBe(0);
    expect(paymentServiceMock.getPaymentRequestsByTeam).toHaveBeenCalled();
  });

  it('should navigate to /my-team-requests/:id on onOpenDetail', () => {
    const request = { id: 7 } as PaymentRequestByTeamDto;

    component.onOpenDetail(request);

    expect(routerMock.navigate).toHaveBeenCalledWith(['/my-team-requests', 7]);
  });

  it('should return 1 for getTotalPages when totalCount is 0', () => {
    component.totalCount = 0;
    component.limit = 10;
    expect(component.getTotalPages()).toBe(1);
  });

  it('should return correct page count for getTotalPages', () => {
    component.totalCount = 25;
    component.limit = 10;
    expect(component.getTotalPages()).toBe(3);
  });

  it('should increment page and reload on nextPage', () => {
    const spy = vi.spyOn(component, 'loadRequests').mockImplementation(() => {});
    component.page = 0;

    component.nextPage();

    expect(component.page).toBe(1);
    expect(spy).toHaveBeenCalled();
  });

  it('should decrement page and reload on previousPage when page > 0', () => {
    const spy = vi.spyOn(component, 'loadRequests').mockImplementation(() => {});
    component.page = 2;

    component.previousPage();

    expect(component.page).toBe(1);
    expect(spy).toHaveBeenCalled();
  });

  it('should not go below page 0 on previousPage', () => {
    const spy = vi.spyOn(component, 'loadRequests').mockImplementation(() => {});
    component.page = 0;

    component.previousPage();

    expect(component.page).toBe(0);
    expect(spy).not.toHaveBeenCalled();
  });
});
