import { ChangeDetectorRef } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';

import { NotificationService } from '../../../services/notification/notification-service';
import { PaymentRequestByUserService } from '../../../services/payment-request-by-user/payment-request-by-user-service';
import { PaymentRequestByUserDto } from '../../../types/exporter';

import { ReceiptOverviewComponent } from './receipt-overview-component';

describe('ReceiptOverviewComponent', () => {
  let component: ReceiptOverviewComponent;
  let fixture: ComponentFixture<ReceiptOverviewComponent>;

  const paymentServiceMock = {
    getPaymentRequestsByUser: vi.fn(),
  };

  const notificationMock = {
    showError: vi.fn(),
  };

  const cdrMock = {
    markForCheck: vi.fn(),
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ReceiptOverviewComponent],
      providers: [
        { provide: PaymentRequestByUserService, useValue: paymentServiceMock },
        { provide: NotificationService, useValue: notificationMock },
        { provide: ChangeDetectorRef, useValue: cdrMock },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ReceiptOverviewComponent);
    component = fixture.componentInstance;
  });

  // -------------------------
  // BASIC
  // -------------------------
  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should call loadRequests on init', () => {
    paymentServiceMock.getPaymentRequestsByUser.mockReturnValue(of({ items: [] }));

    const spy = vi.spyOn(component, 'loadRequests');

    component.ngOnInit();

    expect(spy).toHaveBeenCalled();
  });

  // -------------------------
  // SUCCESS CASE
  // -------------------------
  it('should load requests successfully', () => {
    const apiResponse = {
      items: [
        { id: 1, amount: 100 },
        { id: 2, amount: 200 },
      ] as PaymentRequestByUserDto[],
    };

    paymentServiceMock.getPaymentRequestsByUser.mockReturnValue(of(apiResponse));

    component.loadRequests();

    expect(paymentServiceMock.getPaymentRequestsByUser).toHaveBeenCalledWith({
      IncludeTeam: true,
      IncludeBankAccount: true,
    });

    expect(component.requests).toEqual(apiResponse.items);
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    const cdr = (component as any).cdr;
    const spy = vi.spyOn(cdr, 'markForCheck');

    component.loadRequests();

    expect(spy).toHaveBeenCalled();
  });

  // -------------------------
  // NULL ITEMS
  // -------------------------
  it('should show error if items is null', () => {
    paymentServiceMock.getPaymentRequestsByUser.mockReturnValue(of({ items: null }));

    component.loadRequests();

    expect(notificationMock.showError).toHaveBeenCalledWith('Error loading request.');

    expect(component.requests).toEqual([]);
  });

  // -------------------------
  // ERROR CASE
  // -------------------------
  it('should handle API error', () => {
    paymentServiceMock.getPaymentRequestsByUser.mockReturnValue(throwError(() => 'API error'));

    component.loadRequests();

    expect(notificationMock.showError).toHaveBeenCalledWith('API error');
  });
});
