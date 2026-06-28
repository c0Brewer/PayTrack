import { ChangeDetectorRef } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, Router } from '@angular/router';
import { of, throwError } from 'rxjs';

import { NotificationService } from '../../../../../services/notification/notification-service';
import { PaymentRequestByUserService } from '../../../../../services/payment-request-by-user/payment-request-by-user-service';
import { PaymentRequestByUserDto, TransactionStatus } from '../../../../../types/exporter';

import { MyInvoiceDetailComponent } from './user-detail-component';

describe('MyInvoiceDetailComponent', () => {
  let component: MyInvoiceDetailComponent;
  let fixture: ComponentFixture<MyInvoiceDetailComponent>;

  const serviceMock = {
    getPaymentRequestsByUserById: vi.fn(),
    downloadReceipt: vi.fn(),
  };

  const notificationMock = {
    showError: vi.fn(),
  };

  const routerMock = {
    navigate: vi.fn(),
  };

  const cdrMock = {
    detectChanges: vi.fn(),
  };

  const routeMock = {
    paramMap: of(convertToParamMap({ id: '5' })),
  };

  const mockInvoice = {
    id: 5,
    invoiceNumber: 'INV-005',
    status: 0,
    amount: 50,
    team: { name: 'Engineering' },
    purposeOfPayment: 'Test',
    payoutType: 0,
    comment: '',
    createdAt: '2026-01-01T00:00:00Z',
    paidAt: null,
    bankAccount: { iban: 'AT611904300234573201' },
    user: { name: 'Alice' },
    statusHistory: [],
  } as unknown as PaymentRequestByUserDto;

  beforeEach(async () => {
    vi.clearAllMocks();
    URL.createObjectURL = vi.fn().mockReturnValue('blob:test');
    URL.revokeObjectURL = vi.fn();

    serviceMock.getPaymentRequestsByUserById.mockReturnValue(of(mockInvoice));
    serviceMock.downloadReceipt.mockReturnValue(of(new Blob()));

    await TestBed.configureTestingModule({
      imports: [MyInvoiceDetailComponent],
      providers: [
        { provide: PaymentRequestByUserService, useValue: serviceMock },
        { provide: NotificationService, useValue: notificationMock },
        { provide: ActivatedRoute, useValue: routeMock },
        { provide: Router, useValue: routerMock },
        { provide: ChangeDetectorRef, useValue: cdrMock },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(MyInvoiceDetailComponent);
    component = fixture.componentInstance;
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should call getPaymentRequestsByUserById with the route id on init', () => {
    component.ngOnInit();
    expect(serviceMock.getPaymentRequestsByUserById).toHaveBeenCalledWith(5, expect.any(Object));
  });

  it('should call downloadReceipt with the route id on init', () => {
    component.ngOnInit();
    expect(serviceMock.downloadReceipt).toHaveBeenCalledWith(5);
  });

  it('should set invoice and clear loading on successful invoice load', () => {
    component.ngOnInit();
    expect(component.invoice).toEqual(mockInvoice);
    expect(component.loading).toBe(false);
  });

  it('should return the latest change request message for an invoice requiring changes', () => {
    component.invoice = {
      ...mockInvoice,
      status: TransactionStatus.ChangesRequested,
      statusHistory: [
        {
          fromStatus: TransactionStatus.Submitted,
          toStatus: TransactionStatus.ChangesRequested,
          changedById: 1,
          changedAt: '2026-01-02T00:00:00Z',
          comment: 'First request',
        },
        {
          fromStatus: TransactionStatus.Review,
          toStatus: TransactionStatus.ChangesRequested,
          changedById: 1,
          changedAt: '2026-01-03T00:00:00Z',
          comment: 'Please upload the complete receipt',
        },
      ],
    } as unknown as PaymentRequestByUserDto;

    expect(component.latestChangeRequestMessage).toBe('Please upload the complete receipt');
  });

  it('should not return an old change request message when the invoice no longer requires changes', () => {
    component.invoice = {
      ...mockInvoice,
      status: TransactionStatus.Approved,
      statusHistory: [
        {
          fromStatus: TransactionStatus.Submitted,
          toStatus: TransactionStatus.ChangesRequested,
          changedById: 1,
          changedAt: '2026-01-02T00:00:00Z',
          comment: 'Old request',
        },
      ],
    } as unknown as PaymentRequestByUserDto;

    expect(component.latestChangeRequestMessage).toBeNull();
  });

  it('should render the latest change request message', () => {
    serviceMock.getPaymentRequestsByUserById.mockReturnValue(
      of({
        ...mockInvoice,
        status: TransactionStatus.ChangesRequested,
        statusHistory: [
          {
            fromStatus: TransactionStatus.Submitted,
            toStatus: TransactionStatus.ChangesRequested,
            changedById: 1,
            changedAt: '2026-01-02T00:00:00Z',
            comment: 'Please upload the complete receipt',
          },
        ],
      } as unknown as PaymentRequestByUserDto),
    );

    component.ngOnInit();
    fixture.detectChanges();

    const message = fixture.nativeElement.querySelector('.alert.alert-warning');
    expect(message).not.toBeNull();
    expect(message.textContent).toContain('Please upload the complete receipt');
  });

  it('should render edit action for requested changes without a message', () => {
    serviceMock.getPaymentRequestsByUserById.mockReturnValue(
      of({
        ...mockInvoice,
        status: TransactionStatus.ChangesRequested,
        statusHistory: [],
      } as unknown as PaymentRequestByUserDto),
    );

    component.ngOnInit();
    fixture.detectChanges();

    const action = fixture.nativeElement.querySelector('.alert.alert-warning button');
    const message = fixture.nativeElement.querySelector('.alert.alert-warning p');
    expect(action).not.toBeNull();
    expect(action.textContent).toContain('Revise and Resubmit');
    expect(message).not.toBeNull();
    expect(message.textContent).toContain('Reason:');
  });

  it('should show error and clear loading when invoice load fails', () => {
    serviceMock.getPaymentRequestsByUserById.mockReturnValue(
      throwError(() => new Error('Not found')),
    );
    component.ngOnInit();
    expect(notificationMock.showError).toHaveBeenCalledWith('Could not load invoice: Not found');
    expect(component.loading).toBe(false);
  });

  it('should set rawReceiptBlobUrl and receiptBlobUrl for image blobs', () => {
    serviceMock.downloadReceipt.mockReturnValue(of(new Blob([''], { type: 'image/jpeg' })));
    component.ngOnInit();
    expect(component.rawReceiptBlobUrl).toBe('blob:test');
    expect(component.receiptBlobUrl).toBe('blob:test');
    expect(component.isReceiptImage).toBe(true);
    expect(component.receiptMimeType).toBe('image/jpeg');
  });

  it('should set rawReceiptBlobUrl and receiptBlobUrl for PDF blobs', () => {
    serviceMock.downloadReceipt.mockReturnValue(of(new Blob([''], { type: 'application/pdf' })));
    component.ngOnInit();
    expect(component.rawReceiptBlobUrl).toBe('blob:test');
    expect(component.receiptBlobUrl).toBe('blob:test');
    expect(component.isReceiptImage).toBe(false);
  });

  it('should render img tag in DOM when receipt is an image', () => {
    serviceMock.downloadReceipt.mockReturnValue(of(new Blob([''], { type: 'image/jpeg' })));
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('img.receipt-image')).not.toBeNull();
    expect(fixture.nativeElement.querySelector('iframe.receipt-frame')).toBeNull();
  });

  it('should render iframe in DOM when receipt is a PDF', () => {
    serviceMock.downloadReceipt.mockReturnValue(of(new Blob([''], { type: 'application/pdf' })));
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('iframe.receipt-frame')).not.toBeNull();
    expect(fixture.nativeElement.querySelector('img.receipt-image')).toBeNull();
  });

  it('should set rawReceiptBlobUrl but null receiptBlobUrl for non-displayable blob types', () => {
    serviceMock.downloadReceipt.mockReturnValue(
      of(new Blob([''], { type: 'application/octet-stream' })),
    );
    component.ngOnInit();
    expect(component.rawReceiptBlobUrl).toBe('blob:test');
    expect(component.receiptBlobUrl).toBeNull();
  });

  it('should show error when receipt download fails', () => {
    serviceMock.downloadReceipt.mockReturnValue(throwError(() => new Error('Network error')));
    component.ngOnInit();
    expect(notificationMock.showError).toHaveBeenCalledWith(
      'Could not load receipt: Network error',
    );
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
    expect(mockAnchor.download).toBe('INV-005.jpg');
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

    expect(mockAnchor.download).toBe('INV-005.pdf');
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

  it('onBack navigates to /my-invoices', () => {
    component.onBack();
    expect(routerMock.navigate).toHaveBeenCalledWith(['/my-invoices']);
  });

  it('onEdit navigates to edit route when changes were requested', () => {
    component.invoice = {
      ...mockInvoice,
      status: TransactionStatus.ChangesRequested,
    } as PaymentRequestByUserDto;

    component.onEdit();

    expect(routerMock.navigate).toHaveBeenCalledWith(['/my-invoices', 5, 'edit']);
  });

  it('onEdit does not navigate for other statuses', () => {
    component.invoice = mockInvoice;

    component.onEdit();

    expect(routerMock.navigate).not.toHaveBeenCalled();
  });
});
