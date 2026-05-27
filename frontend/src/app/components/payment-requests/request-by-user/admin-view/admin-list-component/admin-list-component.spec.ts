import { ChangeDetectorRef } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';

import { CostCentreService } from '../../../../../services/cost-centre/cost-centre-service';
import { NotificationService } from '../../../../../services/notification/notification-service';
import { PaymentRequestByUserService } from '../../../../../services/payment-request-by-user/payment-request-by-user-service';
import { TeamService } from '../../../../../services/team/team-service';
import { DuplicatePaymentRequestByUserDto, PaymentRequestByUserDto } from '../../../../../types/exporter';

import { RequestsComponent } from './admin-list-component';

describe('RequestsComponent', () => {
  let component: RequestsComponent;
  let fixture: ComponentFixture<RequestsComponent>;

  const paymentServiceMock = {
    getPaymentRequestsByUser: vi.fn(),
    getDuplicatePaymentRequestsByUser: vi.fn(),
    deletePaymentRequestByUser: vi.fn(),
    dismissDuplicatePaymentRequestByUser: vi.fn(),
  };

  const notificationMock = {
    showError: vi.fn(),
    showSuccess: vi.fn(),
  };

  const routerMock = {
    navigate: vi.fn(),
  };

  const cdrMock = {
    markForCheck: vi.fn(),
  };

  const teamServiceMock = {
    getTeams: vi.fn().mockReturnValue(of({ items: [], totalCount: 0, limit: 1000, offset: 0 })),
  };

  const costCentreServiceMock = {
    getCostCentres: vi
      .fn()
      .mockReturnValue(of({ items: [], totalCount: 0, limit: 1000, offset: 0 })),
  };

  beforeEach(async () => {
    vi.clearAllMocks();
    paymentServiceMock.getDuplicatePaymentRequestsByUser.mockReturnValue(of([]));
    paymentServiceMock.deletePaymentRequestByUser.mockReturnValue(of(undefined));
    paymentServiceMock.dismissDuplicatePaymentRequestByUser.mockReturnValue(of(undefined));

    await TestBed.configureTestingModule({
      imports: [RequestsComponent],
      providers: [
        { provide: PaymentRequestByUserService, useValue: paymentServiceMock },
        { provide: NotificationService, useValue: notificationMock },
        { provide: Router, useValue: routerMock },
        { provide: ChangeDetectorRef, useValue: cdrMock },
        { provide: TeamService, useValue: teamServiceMock },
        { provide: CostCentreService, useValue: costCentreServiceMock },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(RequestsComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    paymentServiceMock.getPaymentRequestsByUser.mockReturnValue(of({ items: [], totalCount: 0 }));
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should load invoices on init', () => {
    paymentServiceMock.getPaymentRequestsByUser.mockReturnValue(of({ items: [], totalCount: 0 }));
    const spy = vi.spyOn(component, 'loadInvoices');
    component.ngOnInit();
    expect(spy).toHaveBeenCalled();
  });

  it('should load invoices successfully', () => {
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

  it('should show error on API failure', () => {
    paymentServiceMock.getPaymentRequestsByUser.mockReturnValue(throwError(() => 'API error'));

    component.loadInvoices();

    expect(notificationMock.showError).toHaveBeenCalledWith('API error');
  });

  it('should reset page to 0 and reload when filter options are updated', () => {
    paymentServiceMock.getPaymentRequestsByUser.mockReturnValue(of({ items: [], totalCount: 0 }));
    component.page = 3;

    component.updateFilterOptions({ Status: 2 as 0 | 1 | 2 | 3 | 4 });

    expect(component.page).toBe(0);
    expect(paymentServiceMock.getPaymentRequestsByUser).toHaveBeenCalled();
  });

  it('should navigate to detail page on open detail', () => {
    const invoice = { id: 1, invoiceNumber: 'INV-001' } as PaymentRequestByUserDto;

    component.onOpenDetail(invoice);

    expect(routerMock.navigate).toHaveBeenCalledWith(['/requests', 1]);
  });

  it('should load duplicates for invoice and filter out current invoice', () => {
    const invoice = {
      id: 1,
      invoiceNumber: 'INV-1',
      amount: 100,
      paidAt: '2026-05-21T00:00:00.000Z',
      team: { id: 3 },
    } as PaymentRequestByUserDto;
    const duplicate = {
      paymentRequestByUser: { id: 2, invoiceNumber: 'INV-2' },
      score: 60,
      isAmountAndUserMatch: false,
      isInvoiceNumberMatch: false,
      isAmountAndTeamMatch: true,
    };
    const selfMatch = {
      paymentRequestByUser: { id: 1, invoiceNumber: 'INV-1' },
      score: 140,
      isAmountAndUserMatch: false,
      isInvoiceNumberMatch: true,
      isAmountAndTeamMatch: true,
    };
    paymentServiceMock.getDuplicatePaymentRequestsByUser.mockReturnValue(
      of([selfMatch, duplicate]),
    );

    component.onOpenDuplicates(invoice);

    expect(paymentServiceMock.getDuplicatePaymentRequestsByUser).toHaveBeenCalledWith({
      TeamId: 3,
      Amount: 100,
      PaidAt: '2026-05-21T00:00:00.000Z',
      InvoiceNumber: 'INV-1',
      PaymentRequestByUserId: 1,
    });
    expect(component.duplicateCandidates).toEqual([duplicate]);
    expect(component.isDuplicateModalOpen).toBe(true);
    expect(component.isDuplicateModalLoading).toBe(false);
  });

  it('should show error when duplicate lookup is missing required data', () => {
    component.onOpenDuplicates({ id: 1, amount: 100 } as PaymentRequestByUserDto);

    expect(notificationMock.showError).toHaveBeenCalledWith(
      'Duplicate lookup is missing team or paid date.',
    );
    expect(paymentServiceMock.getDuplicatePaymentRequestsByUser).not.toHaveBeenCalled();
  });

  it('should delete duplicate invoice after confirmation', () => {
    paymentServiceMock.getPaymentRequestsByUser.mockReturnValue(of({ items: [], totalCount: 0 }));
    vi.spyOn(window, 'confirm').mockReturnValue(true);
    const invoice = { id: 2, invoiceNumber: 'INV-2' } as PaymentRequestByUserDto;

    component.onDeleteDuplicateInvoice(invoice);

    expect(paymentServiceMock.deletePaymentRequestByUser).toHaveBeenCalledWith(2);
    expect(notificationMock.showSuccess).toHaveBeenCalledWith('Invoice deleted.');
    expect(paymentServiceMock.getPaymentRequestsByUser).toHaveBeenCalled();
  });

  it('should dismiss duplicate warning and remove candidate from modal', () => {
    paymentServiceMock.getPaymentRequestsByUser.mockReturnValue(of({ items: [], totalCount: 0 }));
    component.selectedDuplicateInvoice = {
      id: 1,
      invoiceNumber: 'INV-1',
    } as PaymentRequestByUserDto;
    const duplicate = {
      paymentRequestByUser: { id: 2, invoiceNumber: 'INV-2' },
      score: 60,
      isAmountAndUserMatch: false,
      isInvoiceNumberMatch: false,
      isAmountAndTeamMatch: true,
    } as DuplicatePaymentRequestByUserDto;
    component.duplicateCandidates = [duplicate];

    component.onDismissDuplicate(duplicate);

    expect(paymentServiceMock.dismissDuplicatePaymentRequestByUser).toHaveBeenCalledWith(1, 2);
    expect(component.duplicateCandidates).toEqual([]);
    expect(notificationMock.showSuccess).toHaveBeenCalledWith('Duplicate warning dismissed.');
    expect(paymentServiceMock.getPaymentRequestsByUser).toHaveBeenCalled();
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
