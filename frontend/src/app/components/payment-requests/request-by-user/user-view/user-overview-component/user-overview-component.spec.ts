import { ChangeDetectorRef } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { of, throwError } from 'rxjs';

import { AuthService } from '../../../../../services/auth/auth-service';
import { NotificationService } from '../../../../../services/notification/notification-service';
import { PaymentRequestByUserService } from '../../../../../services/payment-request-by-user/payment-request-by-user-service';
import { TeamService } from '../../../../../services/team/team-service';
import {
  PaymentRequestByUserDto,
  Role,
  TransactionStatus,
  UserDto,
} from '../../../../../types/exporter';

import { UserInvoicesOverviewComponent } from './user-overview-component';

describe('UserInvoicesOverviewComponent', () => {
  let component: UserInvoicesOverviewComponent;
  let fixture: ComponentFixture<UserInvoicesOverviewComponent>;

  const paymentServiceMock = {
    getPaymentRequestsByUser: vi.fn(),
  };

  const routerMock = {
    navigate: vi.fn(),
  };

  const activatedRouteMock = {
    snapshot: {
      queryParams: {},
    },
  };

  const notificationMock = {
    showError: vi.fn(),
  };

  const cdrMock = {
    markForCheck: vi.fn(),
  };

  const teamServiceMock = {
    getTeams: vi.fn().mockReturnValue(of({ items: [], totalCount: 0, limit: 1000, offset: 0 })),
  };

  const authServiceMock = {
    getCurrentUser: vi.fn(),
    refreshUser: vi.fn(),
  };

  beforeEach(async () => {
    authServiceMock.getCurrentUser.mockReturnValue(
      of({ id: 7, role: Role.REGULAR_USER } as UserDto),
    );
    authServiceMock.refreshUser.mockResolvedValue({ id: 7, role: Role.REGULAR_USER } as UserDto);
    paymentServiceMock.getPaymentRequestsByUser.mockReturnValue(of({ items: [], totalCount: 0 }));
    activatedRouteMock.snapshot.queryParams = {};

    await TestBed.configureTestingModule({
      imports: [UserInvoicesOverviewComponent],
      providers: [
        { provide: PaymentRequestByUserService, useValue: paymentServiceMock },
        { provide: AuthService, useValue: authServiceMock },
        { provide: NotificationService, useValue: notificationMock },
        { provide: ChangeDetectorRef, useValue: cdrMock },
        { provide: TeamService, useValue: teamServiceMock },
        { provide: ActivatedRoute, useValue: activatedRouteMock },
        { provide: Router, useValue: routerMock },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(UserInvoicesOverviewComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should load invoices on init', async () => {
    const spy = vi.spyOn(component, 'loadInvoices');
    component.ngOnInit();
    await fixture.whenStable();
    expect(spy).toHaveBeenCalled();
  });

  it('should load invoices successfully', () => {
    (component as unknown as { currentUser: UserDto }).currentUser = {
      id: 7,
      role: Role.REGULAR_USER,
    } as UserDto;
    const apiResponse = {
      items: [
        { id: 1, amount: 100 },
        { id: 2, amount: 200 },
      ] as PaymentRequestByUserDto[],
      totalCount: 2,
      hasNext: false,
      hasPrevious: false,
    };
    paymentServiceMock.getPaymentRequestsByUser.mockReturnValue(of(apiResponse));

    component.loadInvoices();

    expect(component.invoices).toEqual(apiResponse.items);
    expect(component.totalCount).toBe(2);
  });

  it('should compute invoice stat boxes from loaded stat invoices', () => {
    component.statInvoices = [
      { id: 1, amount: 100, status: TransactionStatus.Paid },
      { id: 2, amount: 200, status: TransactionStatus.Submitted },
      { id: 3, amount: 300, status: TransactionStatus.Approved },
      { id: 4, amount: 400, status: TransactionStatus.Declined },
    ] as PaymentRequestByUserDto[];

    expect(component.getTotalAmount()).toBe(1000);
    expect(component.getPaidInvoiceCount()).toBe(1);
    expect(component.getOpenInvoiceCount()).toBe(2);
  });

  it('should load stat invoices with the full filtered count', () => {
    (component as unknown as { currentUser: UserDto }).currentUser = {
      id: 7,
      role: Role.REGULAR_USER,
    } as UserDto;
    const apiResponse = {
      items: [{ id: 1, amount: 100, status: TransactionStatus.Paid }] as PaymentRequestByUserDto[],
      totalCount: 12,
      hasNext: true,
      hasPrevious: false,
    };
    paymentServiceMock.getPaymentRequestsByUser.mockReturnValue(of(apiResponse));

    component.loadInvoices();

    expect(paymentServiceMock.getPaymentRequestsByUser).toHaveBeenLastCalledWith(
      expect.objectContaining({ Limit: 12, Offset: 0 }),
    );
  });

  it('should filter my invoices by current admin user id', async () => {
    const admin = { id: 7, role: Role.ADMIN } as UserDto;
    const apiResponse = {
      items: [{ id: 1, amount: 100 }] as PaymentRequestByUserDto[],
      totalCount: 1,
      hasNext: false,
      hasPrevious: false,
    };
    authServiceMock.refreshUser.mockResolvedValue(admin);
    paymentServiceMock.getPaymentRequestsByUser.mockReturnValue(of(apiResponse));

    component.ngOnInit();
    await fixture.whenStable();

    expect(paymentServiceMock.getPaymentRequestsByUser).toHaveBeenCalledWith(
      expect.objectContaining({ UserId: admin.id }),
    );
  });

  it('should apply filter options from query params on init', () => {
    const user = { id: 7 } as UserDto;
    authServiceMock.getCurrentUser.mockReturnValue(of(user));
    activatedRouteMock.snapshot.queryParams = {
      status: '2',
      purposeOfPayment: 'Hardware',
      minAmount: '25',
    };

    component.ngOnInit();

    expect(component.filterOptions).toEqual(
      expect.objectContaining({
        IncludeTeam: true,
        Status: 2,
        PurposeOfPayment: 'Hardware',
        MinAmount: 25,
      }),
    );
    expect(paymentServiceMock.getPaymentRequestsByUser).toHaveBeenCalledWith(
      expect.objectContaining({
        UserId: user.id,
        Status: 2,
        PurposeOfPayment: 'Hardware',
        MinAmount: 25,
      }),
    );
  });

  it('should show error on API failure', () => {
    (component as unknown as { currentUser: UserDto }).currentUser = {
      id: 7,
      role: Role.REGULAR_USER,
    } as UserDto;
    paymentServiceMock.getPaymentRequestsByUser.mockReturnValue(throwError(() => 'API error'));

    component.loadInvoices();

    expect(notificationMock.showError).toHaveBeenCalledWith('API error');
  });

  it('should reset page to 0 and reload when filter options are updated', () => {
    (component as unknown as { currentUser: UserDto }).currentUser = {
      id: 7,
      role: Role.REGULAR_USER,
    } as UserDto;
    component.page = 3;

    component.updateFilterOptions({ Status: 2 as 0 | 1 | 2 | 3 | 4 });

    expect(component.page).toBe(0);
    expect(paymentServiceMock.getPaymentRequestsByUser).toHaveBeenCalled();
  });

  it('should navigate to detail page on open detail', () => {
    const invoice = { id: 1, invoiceNumber: 'INV-001' } as PaymentRequestByUserDto;

    component.onOpenDetail(invoice);

    expect(routerMock.navigate).toHaveBeenCalledWith(['/my-invoices', 1]);
  });

  it('should return 1 for getTotalPages when totalCount is 0', () => {
    component.totalCount = 0;
    component.limit = 10;
    expect(component.getTotalPages()).toBe(1);
  });

  it('should increment page and reload on nextPage', () => {
    const spy = vi.spyOn(component, 'loadInvoices').mockImplementation(() => {});
    component.page = 0;

    component.nextPage();

    expect(component.page).toBe(1);
    expect(spy).toHaveBeenCalled();
  });

  it('should decrement page and reload on previousPage when page > 0', () => {
    const spy = vi.spyOn(component, 'loadInvoices').mockImplementation(() => {});
    component.page = 2;

    component.previousPage();

    expect(component.page).toBe(1);
    expect(spy).toHaveBeenCalled();
  });

  it('should not go below page 0 on previousPage', () => {
    const spy = vi.spyOn(component, 'loadInvoices').mockImplementation(() => {});
    component.page = 0;

    component.previousPage();

    expect(component.page).toBe(0);
    expect(spy).not.toHaveBeenCalled();
  });
});
