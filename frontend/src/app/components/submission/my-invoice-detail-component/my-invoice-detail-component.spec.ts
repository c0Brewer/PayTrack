import { ChangeDetectorRef } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, Router } from '@angular/router';
import { of, throwError } from 'rxjs';

import { NotificationService } from '../../../services/notification/notification-service';
import { PaymentRequestByUserService } from '../../../services/payment-request-by-user/payment-request-by-user-service';
import { PaymentRequestByUserDto } from '../../../types/exporter';

import { MyInvoiceDetailComponent } from './my-invoice-detail-component';

describe('MyInvoiceDetailComponent', () => {
  let component: MyInvoiceDetailComponent;
  let fixture: ComponentFixture<MyInvoiceDetailComponent>;

  const serviceMock = {
    getMyInvoiceById: vi.fn(),
    downloadMyReceipt: vi.fn(),
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

  const mockInvoice = { id: 5, invoiceNumber: 'INV-005' } as PaymentRequestByUserDto;

  beforeEach(async () => {
    vi.clearAllMocks();
    URL.createObjectURL = vi.fn().mockReturnValue('blob:test');
    URL.revokeObjectURL = vi.fn();

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
    serviceMock.getMyInvoiceById.mockReturnValue(of(mockInvoice));
    serviceMock.downloadMyReceipt.mockReturnValue(of(new Blob()));
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should call getMyInvoiceById with the route id on init', () => {
    serviceMock.getMyInvoiceById.mockReturnValue(of(mockInvoice));
    serviceMock.downloadMyReceipt.mockReturnValue(of(new Blob()));
    component.ngOnInit();
    expect(serviceMock.getMyInvoiceById).toHaveBeenCalledWith(5, expect.any(Object));
  });

  it('should call downloadMyReceipt with the route id on init', () => {
    serviceMock.getMyInvoiceById.mockReturnValue(of(mockInvoice));
    serviceMock.downloadMyReceipt.mockReturnValue(of(new Blob()));
    component.ngOnInit();
    expect(serviceMock.downloadMyReceipt).toHaveBeenCalledWith(5);
  });

  it('should set invoice and clear loading on successful invoice load', () => {
    serviceMock.getMyInvoiceById.mockReturnValue(of(mockInvoice));
    serviceMock.downloadMyReceipt.mockReturnValue(of(new Blob()));
    component.ngOnInit();
    expect(component.invoice).toEqual(mockInvoice);
    expect(component.loading).toBe(false);
  });

  it('should show error and clear loading when invoice load fails', () => {
    serviceMock.getMyInvoiceById.mockReturnValue(throwError(() => new Error('Not found')));
    serviceMock.downloadMyReceipt.mockReturnValue(of(new Blob()));
    component.ngOnInit();
    expect(notificationMock.showError).toHaveBeenCalledWith('Could not load invoice: Not found');
    expect(component.loading).toBe(false);
  });

  it('should set rawReceiptBlobUrl and receiptBlobUrl for image blobs', () => {
    serviceMock.getMyInvoiceById.mockReturnValue(of(mockInvoice));
    serviceMock.downloadMyReceipt.mockReturnValue(of(new Blob([''], { type: 'image/jpeg' })));
    component.ngOnInit();
    expect(component.rawReceiptBlobUrl).toBe('blob:test');
    expect(component.receiptBlobUrl).toBe('blob:test');
    expect(component.isReceiptImage).toBe(true);
    expect(component.receiptMimeType).toBe('image/jpeg');
  });

  it('should set rawReceiptBlobUrl and receiptBlobUrl for PDF blobs', () => {
    serviceMock.getMyInvoiceById.mockReturnValue(of(mockInvoice));
    serviceMock.downloadMyReceipt.mockReturnValue(of(new Blob([''], { type: 'application/pdf' })));
    component.ngOnInit();
    expect(component.rawReceiptBlobUrl).toBe('blob:test');
    expect(component.receiptBlobUrl).toBe('blob:test');
    expect(component.isReceiptImage).toBe(false);
  });

  it('should set rawReceiptBlobUrl but null receiptBlobUrl for non-displayable blob types', () => {
    serviceMock.getMyInvoiceById.mockReturnValue(of(mockInvoice));
    serviceMock.downloadMyReceipt.mockReturnValue(
      of(new Blob([''], { type: 'application/octet-stream' })),
    );
    component.ngOnInit();
    expect(component.rawReceiptBlobUrl).toBe('blob:test');
    expect(component.receiptBlobUrl).toBeNull();
  });

  it('should show error when receipt download fails', () => {
    serviceMock.getMyInvoiceById.mockReturnValue(of(mockInvoice));
    serviceMock.downloadMyReceipt.mockReturnValue(throwError(() => new Error('Network error')));
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
});
