import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';

import { HomeDashboardService } from '../../../services/home/home-dashboard-service';
import { NotificationService } from '../../../services/notification/notification-service';
import { Role, TransactionStatus } from '../../../types/exporter';

import { HomeComponent } from './home-component';

describe('HomeComponent', () => {
  let component: HomeComponent;
  let fixture: ComponentFixture<HomeComponent>;
  const dashboardResponse = {
    user: { id: 1, name: 'Alex', role: Role.REGULAR_USER },
    invoices: {
      openCount: 1,
      submittedCount: 2,
      paidCount: 3,
      openAmount: 100,
      lastPaidAt: null,
      totalRecentCount: 1,
      recent: [],
    },
    paymentRequests: {
      openCount: 4,
      submittedCount: 5,
      paidCount: 6,
      openAmount: 200,
      lastPaidAt: null,
      totalRecentCount: 6,
      recent: [],
    },
    actions: {
      missingBankAccount: false,
      bankInformationSkipped: false,
      needsAttentionCount: 0,
    },
  };

  const homeDashboardServiceMock = {
    getHomeDashboard: vi.fn().mockReturnValue(of(dashboardResponse)),
  };

  const notificationServiceMock = {
    showError: vi.fn(),
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [HomeComponent],
      providers: [
        provideRouter([]),
        { provide: HomeDashboardService, useValue: homeDashboardServiceMock },
        { provide: NotificationService, useValue: notificationServiceMock },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(HomeComponent);
    component = fixture.componentInstance;
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should load the dashboard on init', () => {
    fixture.detectChanges();

    expect(homeDashboardServiceMock.getHomeDashboard).toHaveBeenCalledOnce();
    expect(component.dashboard).toEqual(dashboardResponse);
    expect(component.isLoading).toBe(false);
  });

  it('should show an error when loading the dashboard fails', () => {
    homeDashboardServiceMock.getHomeDashboard.mockReturnValueOnce(
      throwError(() => new Error('Dashboard failed')),
    );

    fixture.detectChanges();

    expect(notificationServiceMock.showError).toHaveBeenCalledWith('Dashboard failed');
    expect(component.isLoading).toBe(false);
  });

  it('should build the welcome message from the dashboard user', () => {
    component.dashboard = dashboardResponse;

    expect(component.welcomeMessage).toContain('Alex');
  });

  it('should expose transaction status helpers', () => {
    expect(component.getStatusLabel(TransactionStatus.Paid)).toBe('Paid');
    expect(component.getStatusClass(TransactionStatus.Paid)).toBe('status-paid');
  });

  it('should derive readable recent item labels', () => {
    expect(
      component.getInvoiceReference({
        id: 1,
        amount: 12,
        status: TransactionStatus.Submitted,
        createdAt: null,
        paidAt: null,
        reference: 'INV-1',
        purposeOfPayment: 'Office supplies',
        teamName: null,
        userName: null,
      }),
    ).toBe('INV-1');

    expect(
      component.getPaymentRequestReference({
        id: 1,
        amount: 12,
        status: TransactionStatus.Submitted,
        createdAt: null,
        paidAt: null,
        reference: null,
        purposeOfPayment: 'Budget refill',
        teamName: null,
        userName: null,
      }),
    ).toBe('Budget refill');
  });

  it('should show a note when more recent entries exist than displayed', () => {
    component.dashboard = {
      ...dashboardResponse,
      paymentRequests: {
        ...dashboardResponse.paymentRequests,
        totalRecentCount: 6,
        recent: [
          {
            id: 1,
            amount: 12,
            status: TransactionStatus.Submitted,
            createdAt: null,
            paidAt: null,
            reference: null,
            purposeOfPayment: 'Budget refill',
            teamName: null,
            userName: null,
          },
        ],
      },
    };
    component.isLoading = false;

    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('5 more entries available!');
  });
});
