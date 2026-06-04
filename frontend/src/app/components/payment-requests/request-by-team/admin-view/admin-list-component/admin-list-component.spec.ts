import { ChangeDetectorRef } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';

import { NotificationService } from '../../../../../services/notification/notification-service';
import { PaymentRequestByTeamService } from '../../../../../services/payment-request-by-team/payment-request-by-team-service';
import { TeamService } from '../../../../../services/team/team-service';
import { UserService } from '../../../../../services/user/user-service';
import { PaymentRequestByTeamDto, TransactionStatus } from '../../../../../types/exporter';

import { TeamRequestsComponent } from './admin-list-component';

describe('TeamRequestsComponent', () => {
  let component: TeamRequestsComponent;
  let fixture: ComponentFixture<TeamRequestsComponent>;

  const paymentServiceMock = {
    getPaymentRequestsByTeam: vi.fn(),
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
      imports: [TeamRequestsComponent],
      providers: [
        { provide: PaymentRequestByTeamService, useValue: paymentServiceMock },
        { provide: NotificationService, useValue: notificationMock },
        { provide: Router, useValue: routerMock },
        { provide: ChangeDetectorRef, useValue: cdrMock },
        { provide: TeamService, useValue: teamServiceMock },
        { provide: UserService, useValue: userServiceMock },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(TeamRequestsComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    paymentServiceMock.getPaymentRequestsByTeam.mockReturnValue(
      of({ items: [], totalCount: 0, hasNext: false, hasPrevious: false }),
    );
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should call loadRequests on init', () => {
    paymentServiceMock.getPaymentRequestsByTeam.mockReturnValue(
      of({ items: [], totalCount: 0, hasNext: false, hasPrevious: false }),
    );
    const spy = vi.spyOn(component, 'loadRequests');
    component.ngOnInit();
    expect(spy).toHaveBeenCalled();
  });

  it('should set requests and totalCount on successful load', () => {
    const items = [
      { id: 1, amount: 100, status: TransactionStatus.Submitted },
      { id: 2, amount: 200, status: TransactionStatus.Paid },
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

  it('should navigate to detail page on onOpenDetail', () => {
    const request = { id: 7 } as PaymentRequestByTeamDto;

    component.onOpenDetail(request);

    expect(routerMock.navigate).toHaveBeenCalledWith(['/payment-requests-by-team', 7]);
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
