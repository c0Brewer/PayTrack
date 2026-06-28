import { ChangeDetectorRef } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, Router } from '@angular/router';
import { of, throwError } from 'rxjs';

import { BudgetService } from '../../../../../services/budget/budget-service';
import { ExternalNotificationService } from '../../../../../services/external-notification/external-notification-service';
import { NotificationService } from '../../../../../services/notification/notification-service';
import { PaymentRequestByUserService } from '../../../../../services/payment-request-by-user/payment-request-by-user-service';
import { PaymentRequestStatusRefreshService } from '../../../../../services/payment-request-by-user/payment-request-status-refresh-service';
import { PaymentRequestByUserDto, TransactionStatus } from '../../../../../types/exporter';

import { RequestDetailComponent } from './admin-detail-component';

describe('RequestDetailComponent', () => {
  let component: RequestDetailComponent;
  let fixture: ComponentFixture<RequestDetailComponent>;

  const serviceMock = {
    getPaymentRequestsByUserById: vi.fn(),
    downloadReceipt: vi.fn(),
    markPaymentRequestByUserAsPaid: vi.fn(),
    approvePaymentRequestByUser: vi.fn(),
    declinePaymentRequestByUser: vi.fn(),
    requestChangesForPaymentRequestByUser: vi.fn(),
    undoLastStatusChange: vi.fn(),
  };

  const budgetServiceMock = {
    getBudgets: vi.fn(),
  };

  const notificationMock = {
    showError: vi.fn(),
    showSuccess: vi.fn(),
  };

  const statusRefreshMock = {
    requestRefresh: vi.fn(),
  };

  const externalNotificationMock = {
    sendEmail: vi.fn(),
    sendSlack: vi.fn(),
  };

  const routerMock = {
    navigate: vi.fn(),
  };

  const cdrMock = {
    detectChanges: vi.fn(),
  };

  const routeMock = {
    paramMap: of(convertToParamMap({ id: '7' })),
  };

  const mockInvoice = {
    id: 7,
    invoiceNumber: 'INV-007',
    status: 0,
    amount: 100,
    team: { id: 3, name: 'Finance' },
    purposeOfPayment: 'Office supplies',
    payoutType: 0,
    comment: '',
    createdAt: '2026-01-01T00:00:00Z',
    paidAt: null,
    bankAccount: { iban: 'AT611904300234573201' },
    user: { id: 8, name: 'Bob Admin', email: 'bob@example.com' },
    statusHistory: [],
  } as unknown as PaymentRequestByUserDto;

  beforeEach(async () => {
    vi.clearAllMocks();
    URL.createObjectURL = vi.fn().mockReturnValue('blob:test');
    URL.revokeObjectURL = vi.fn();
    budgetServiceMock.getBudgets.mockReturnValue(of({ items: [] }));

    await TestBed.configureTestingModule({
      imports: [RequestDetailComponent],
      providers: [
        { provide: PaymentRequestByUserService, useValue: serviceMock },
        { provide: BudgetService, useValue: budgetServiceMock },
        { provide: ExternalNotificationService, useValue: externalNotificationMock },
        { provide: NotificationService, useValue: notificationMock },
        { provide: PaymentRequestStatusRefreshService, useValue: statusRefreshMock },
        { provide: ActivatedRoute, useValue: routeMock },
        { provide: Router, useValue: routerMock },
        { provide: ChangeDetectorRef, useValue: cdrMock },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(RequestDetailComponent);
    component = fixture.componentInstance;
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('should create', () => {
    serviceMock.getPaymentRequestsByUserById.mockReturnValue(of(mockInvoice));
    serviceMock.downloadReceipt.mockReturnValue(of(new Blob()));
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should call getPaymentRequestsByUserById with the route id on init', () => {
    serviceMock.getPaymentRequestsByUserById.mockReturnValue(of(mockInvoice));
    serviceMock.downloadReceipt.mockReturnValue(of(new Blob()));
    component.ngOnInit();
    expect(serviceMock.getPaymentRequestsByUserById).toHaveBeenCalledWith(7, expect.any(Object));
  });

  it('should call getPaymentRequestsByUserById with all includes on init', () => {
    serviceMock.getPaymentRequestsByUserById.mockReturnValue(of(mockInvoice));
    serviceMock.downloadReceipt.mockReturnValue(of(new Blob()));
    component.ngOnInit();
    expect(serviceMock.getPaymentRequestsByUserById).toHaveBeenCalledWith(7, {
      IncludeUser: true,
      IncludeTeam: true,
      IncludeBankAccount: true,
      IncludeStatusHistory: true,
    });
  });

  it('should load budgets for the invoice team on init', () => {
    const budget = {
      id: 9,
      name: 'Operations 2026',
      teamId: 3,
      periodStart: '2026-01-01T00:00:00Z',
      periodEnd: '2026-12-31T00:00:00Z',
    };
    budgetServiceMock.getBudgets.mockReturnValue(
      of({
        items: [budget],
      }),
    );
    serviceMock.getPaymentRequestsByUserById.mockReturnValue(of(mockInvoice));
    serviceMock.downloadReceipt.mockReturnValue(of(new Blob()));

    component.ngOnInit();

    expect(budgetServiceMock.getBudgets).toHaveBeenCalledWith({ TeamId: 3, Limit: 100 });
    expect(component.budgets).toEqual([budget]);
  });

  it('should call downloadReceipt with the route id on init', () => {
    serviceMock.getPaymentRequestsByUserById.mockReturnValue(of(mockInvoice));
    serviceMock.downloadReceipt.mockReturnValue(of(new Blob()));
    component.ngOnInit();
    expect(serviceMock.downloadReceipt).toHaveBeenCalledWith(7);
  });

  it('should set invoice and clear loading on successful invoice load', () => {
    serviceMock.getPaymentRequestsByUserById.mockReturnValue(of(mockInvoice));
    serviceMock.downloadReceipt.mockReturnValue(of(new Blob()));
    component.ngOnInit();
    expect(component.invoice).toEqual(mockInvoice);
    expect(component.loading).toBe(false);
  });

  it('should show error and clear loading when invoice load fails', () => {
    serviceMock.getPaymentRequestsByUserById.mockReturnValue(
      throwError(() => new Error('Not found')),
    );
    serviceMock.downloadReceipt.mockReturnValue(of(new Blob()));
    component.ngOnInit();
    expect(notificationMock.showError).toHaveBeenCalledWith('Could not load invoice: Not found');
    expect(component.loading).toBe(false);
  });

  it('should set rawReceiptBlobUrl and receiptBlobUrl for image blobs', () => {
    serviceMock.getPaymentRequestsByUserById.mockReturnValue(of(mockInvoice));
    serviceMock.downloadReceipt.mockReturnValue(of(new Blob([''], { type: 'image/jpeg' })));
    component.ngOnInit();
    expect(component.rawReceiptBlobUrl).toBe('blob:test');
    expect(component.receiptBlobUrl).toBe('blob:test');
    expect(component.isReceiptImage).toBe(true);
    expect(component.receiptMimeType).toBe('image/jpeg');
  });

  it('should set rawReceiptBlobUrl and receiptBlobUrl for PDF blobs', () => {
    serviceMock.getPaymentRequestsByUserById.mockReturnValue(of(mockInvoice));
    serviceMock.downloadReceipt.mockReturnValue(of(new Blob([''], { type: 'application/pdf' })));
    component.ngOnInit();
    expect(component.rawReceiptBlobUrl).toBe('blob:test');
    expect(component.receiptBlobUrl).toBe('blob:test');
    expect(component.isReceiptImage).toBe(false);
  });

  it('should render img tag in DOM when receipt is an image', () => {
    serviceMock.getPaymentRequestsByUserById.mockReturnValue(of(mockInvoice));
    serviceMock.downloadReceipt.mockReturnValue(of(new Blob([''], { type: 'image/jpeg' })));
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('img.receipt-image')).not.toBeNull();
    expect(fixture.nativeElement.querySelector('iframe.receipt-frame')).toBeNull();
  });

  it('should render iframe in DOM when receipt is a PDF', () => {
    serviceMock.getPaymentRequestsByUserById.mockReturnValue(of(mockInvoice));
    serviceMock.downloadReceipt.mockReturnValue(of(new Blob([''], { type: 'application/pdf' })));
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('iframe.receipt-frame')).not.toBeNull();
    expect(fixture.nativeElement.querySelector('img.receipt-image')).toBeNull();
  });

  it('should set rawReceiptBlobUrl but null receiptBlobUrl for non-displayable blob types', () => {
    serviceMock.getPaymentRequestsByUserById.mockReturnValue(of(mockInvoice));
    serviceMock.downloadReceipt.mockReturnValue(
      of(new Blob([''], { type: 'application/octet-stream' })),
    );
    component.ngOnInit();
    expect(component.rawReceiptBlobUrl).toBe('blob:test');
    expect(component.receiptBlobUrl).toBeNull();
  });

  it('should show error when receipt download fails', () => {
    serviceMock.getPaymentRequestsByUserById.mockReturnValue(of(mockInvoice));
    serviceMock.downloadReceipt.mockReturnValue(throwError(() => new Error('Network error')));
    component.ngOnInit();
    expect(notificationMock.showError).toHaveBeenCalledWith(
      'Could not load receipt: Network error',
    );
  });

  it('should reload invoice with includes after mark paid succeeds', () => {
    const reloadedInvoice = {
      ...mockInvoice,
      status: 3,
      team: { name: 'Finance reloaded' },
    } as unknown as PaymentRequestByUserDto;
    component.invoice = mockInvoice;
    serviceMock.markPaymentRequestByUserAsPaid.mockReturnValue(of({ id: 7 }));
    serviceMock.getPaymentRequestsByUserById.mockReturnValue(of(reloadedInvoice));

    component.onMarkPaid({
      paymentReference: 'REF-123',
      purposeOfPayment: 'Supplier payout',
      paymentDate: '2026-02-03T00:00:00.000Z',
    });

    expect(serviceMock.markPaymentRequestByUserAsPaid).toHaveBeenCalledWith(7, {
      paymentReference: 'REF-123',
      purposeOfPayment: 'Supplier payout',
      paymentDate: '2026-02-03T00:00:00.000Z',
    });
    expect(serviceMock.getPaymentRequestsByUserById).toHaveBeenCalledWith(7, {
      IncludeUser: true,
      IncludeTeam: true,
      IncludeBankAccount: true,
      IncludeStatusHistory: true,
    });
    expect(component.invoice).toEqual(reloadedInvoice);
    expect(notificationMock.showSuccess).toHaveBeenCalledWith('Invoice marked as paid');
  });

  it('should reload invoice with includes after approve succeeds', () => {
    const reloadedInvoice = {
      ...mockInvoice,
      status: 2,
      team: { name: 'Finance reloaded' },
    } as unknown as PaymentRequestByUserDto;
    component.invoice = mockInvoice;
    serviceMock.approvePaymentRequestByUser.mockReturnValue(of({ id: 7 }));
    serviceMock.getPaymentRequestsByUserById.mockReturnValue(of(reloadedInvoice));

    component.onApprove({ budgetId: 5, reason: 'valid reason' });

    expect(serviceMock.approvePaymentRequestByUser).toHaveBeenCalledWith(7, {
      budgetId: 5,
      reason: 'valid reason',
    });
    expect(serviceMock.getPaymentRequestsByUserById).toHaveBeenCalledWith(7, {
      IncludeUser: true,
      IncludeTeam: true,
      IncludeBankAccount: true,
      IncludeStatusHistory: true,
    });
    expect(component.invoice).toEqual(reloadedInvoice);
    expect(notificationMock.showSuccess).toHaveBeenCalledWith('Invoice approved');
  });

  it('should revoke blob URL on destroy when one exists', () => {
    component.rawReceiptBlobUrl = 'blob:existing';
    component.ngOnDestroy();
    expect(URL.revokeObjectURL).toHaveBeenCalledWith('blob:existing');
  });

  it('should not call revokeObjectURL on destroy when no URL exists', () => {
    component.rawReceiptBlobUrl = null;
    component.ngOnDestroy();
    expect(URL.revokeObjectURL).not.toHaveBeenCalled();
  });

  it('onDownloadReceipt does nothing when rawReceiptBlobUrl is null', () => {
    const spy = vi.spyOn(document.body, 'appendChild');
    component.rawReceiptBlobUrl = null;
    component.onDownloadReceipt();
    expect(spy).not.toHaveBeenCalled();
  });

  it('onDownloadReceipt creates anchor with JPEG filename and clicks it', () => {
    component.rawReceiptBlobUrl = 'blob:test';
    component.receiptMimeType = 'image/jpeg';
    component.invoice = mockInvoice;

    const mockAnchor = {
      style: {} as CSSStyleDeclaration,
      href: '',
      download: '',
      click: vi.fn(),
    } as unknown as HTMLAnchorElement;
    vi.spyOn(document, 'createElement').mockReturnValue(mockAnchor);
    vi.spyOn(document.body, 'appendChild').mockImplementation(() => mockAnchor);
    vi.spyOn(document.body, 'removeChild').mockImplementation(() => mockAnchor);

    component.onDownloadReceipt();

    expect(mockAnchor.href).toBe('blob:test');
    expect(mockAnchor.download).toBe('INV-007.jpg');
    expect(mockAnchor.click).toHaveBeenCalled();
  });

  it('onDownloadReceipt creates anchor with PDF filename', () => {
    component.rawReceiptBlobUrl = 'blob:test';
    component.receiptMimeType = 'application/pdf';
    component.invoice = mockInvoice;

    const mockAnchor = {
      style: {} as CSSStyleDeclaration,
      href: '',
      download: '',
      click: vi.fn(),
    } as unknown as HTMLAnchorElement;
    vi.spyOn(document, 'createElement').mockReturnValue(mockAnchor);
    vi.spyOn(document.body, 'appendChild').mockImplementation(() => mockAnchor);
    vi.spyOn(document.body, 'removeChild').mockImplementation(() => mockAnchor);

    component.onDownloadReceipt();

    expect(mockAnchor.download).toBe('INV-007.pdf');
    expect(mockAnchor.click).toHaveBeenCalled();
  });

  it('onDownloadReceipt uses "receipt" as fallback filename when invoice is null', () => {
    component.rawReceiptBlobUrl = 'blob:test';
    component.receiptMimeType = 'image/png';
    component.invoice = null;

    const mockAnchor = {
      style: {} as CSSStyleDeclaration,
      href: '',
      download: '',
      click: vi.fn(),
    } as unknown as HTMLAnchorElement;
    vi.spyOn(document, 'createElement').mockReturnValue(mockAnchor);
    vi.spyOn(document.body, 'appendChild').mockImplementation(() => mockAnchor);
    vi.spyOn(document.body, 'removeChild').mockImplementation(() => mockAnchor);

    component.onDownloadReceipt();

    expect(mockAnchor.download).toBe('receipt.png');
  });

  it('onBack navigates to /requests', () => {
    component.onBack();
    expect(routerMock.navigate).toHaveBeenCalledWith(['/requests']);
  });

  it('returns the invoice user email for notifications', () => {
    component.invoice = mockInvoice;
    expect(component.notificationEmail).toBe('bob@example.com');
  });

  it('returns a changes requested subject containing the invoice number', () => {
    component.invoice = mockInvoice;
    expect(component.notificationSubject).toContain('INV-007');
  });

  it('returns an email notification message with invoice context and latest change reason', () => {
    component.invoice = {
      ...mockInvoice,
      statusHistory: [
        {
          fromStatus: TransactionStatus.Submitted,
          toStatus: TransactionStatus.ChangesRequested,
          changedAt: '2026-01-02T00:00:00Z',
          changedById: 1,
          comment: 'Please upload a clearer receipt',
        },
      ],
    } as unknown as PaymentRequestByUserDto;
    component.modalType = 'email';

    const message = component.notificationMessage;

    expect(message).toContain('Bob Admin');
    expect(message).toContain('INV-007');
    expect(message).toContain('Please upload a clearer receipt');
  });

  it('returns a slack notification message with invoice context', () => {
    component.invoice = mockInvoice;
    component.modalType = 'slack';

    const message = component.notificationMessage;

    expect(message).toContain('INV-007');
    expect(message).toContain('100');
  });

  it('requests changes before opening the email notification modal', () => {
    const reloadedInvoice = {
      ...mockInvoice,
      status: TransactionStatus.ChangesRequested,
    } as unknown as PaymentRequestByUserDto;
    component.invoice = mockInvoice;
    serviceMock.requestChangesForPaymentRequestByUser.mockReturnValue(of({ id: 7 }));
    serviceMock.getPaymentRequestsByUserById.mockReturnValue(of(reloadedInvoice));

    component.onRequestChanges({
      reason: 'Please upload a clearer receipt',
      contactMethod: 'email',
    });

    expect(serviceMock.requestChangesForPaymentRequestByUser).toHaveBeenCalledWith(7, {
      reason: 'Please upload a clearer receipt',
    });
    expect(component.modalType).toBe('email');
    expect(component.pendingChangeRequest).toEqual({
      reason: 'Please upload a clearer receipt',
    });
    expect(component.notificationMessage).toContain('Please upload a clearer receipt');
  });

  it('does not open the notification modal when request changes fails', () => {
    component.invoice = mockInvoice;
    serviceMock.requestChangesForPaymentRequestByUser.mockReturnValue(
      throwError(() => new Error('status failed')),
    );

    component.onRequestChanges({
      reason: 'Please upload a clearer receipt',
      contactMethod: 'email',
    });

    expect(component.modalType).toBeNull();
    expect(component.pendingChangeRequest).toBeNull();
    expect(notificationMock.showError).toHaveBeenCalledWith(
      'Could not request changes: status failed',
    );
  });

  it('opens the slack notification modal after request changes succeeds', () => {
    component.invoice = mockInvoice;
    serviceMock.requestChangesForPaymentRequestByUser.mockReturnValue(of({ id: 7 }));
    serviceMock.getPaymentRequestsByUserById.mockReturnValue(of(mockInvoice));

    component.onRequestChanges({
      reason: 'Please upload a clearer receipt',
      contactMethod: 'slack',
    });

    expect(serviceMock.requestChangesForPaymentRequestByUser).toHaveBeenCalledWith(7, {
      reason: 'Please upload a clearer receipt',
    });
    expect(component.modalType).toBe('slack');
  });

  it('clears pending request when the notification modal is closed', () => {
    component.invoice = mockInvoice;
    serviceMock.requestChangesForPaymentRequestByUser.mockReturnValue(of({ id: 7 }));
    serviceMock.getPaymentRequestsByUserById.mockReturnValue(of(mockInvoice));

    component.onRequestChanges({
      reason: 'Please upload a clearer receipt',
      contactMethod: 'email',
    });

    component.onNotificationModalClosed();

    expect(component.modalType).toBeNull();
    expect(component.pendingChangeRequest).toBeNull();
  });

  it('shows undo button after a status action succeeds', () => {
    component.invoice = mockInvoice;
    serviceMock.declinePaymentRequestByUser.mockReturnValue(of({ id: 7 }));
    serviceMock.getPaymentRequestsByUserById.mockReturnValue(of(mockInvoice));

    component.onDecline({ reason: 'duplicate' });

    expect(component.canUndoLastStatusChange).toBe(true);
  });

  it('undoes the last status change and reloads the invoice', () => {
    const reloadedInvoice = {
      ...mockInvoice,
      status: TransactionStatus.Submitted,
    } as unknown as PaymentRequestByUserDto;
    component.invoice = mockInvoice;
    component.canUndoLastStatusChange = true;
    serviceMock.undoLastStatusChange.mockReturnValue(of({ id: 7 }));
    serviceMock.getPaymentRequestsByUserById.mockReturnValue(of(reloadedInvoice));

    component.onUndoStatusChange();

    expect(serviceMock.undoLastStatusChange).toHaveBeenCalledWith(7);
    expect(component.invoice).toEqual(reloadedInvoice);
    expect(component.canUndoLastStatusChange).toBe(false);
    expect(notificationMock.showSuccess).toHaveBeenCalledWith('Status change undone');
  });
});
