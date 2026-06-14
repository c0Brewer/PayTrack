import { ChangeDetectorRef } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, Router } from '@angular/router';
import { of, throwError } from 'rxjs';

import { NotificationService } from '../../../../../services/notification/notification-service';
import { PaymentRequestByTeamService } from '../../../../../services/payment-request-by-team/payment-request-by-team-service';
import { PaymentRequestByTeamDto, TransactionStatus } from '../../../../../types/exporter';

import { TeamRequestAdminDetailComponent } from './admin-detail-component';

describe('TeamRequestAdminDetailComponent', () => {
  let component: TeamRequestAdminDetailComponent;
  let fixture: ComponentFixture<TeamRequestAdminDetailComponent>;

  const serviceMock = {
    getPaymentRequestsByTeamById: vi.fn(),
    markAsPaid: vi.fn(),
    deletePaymentRequestByTeam: vi.fn(),
  };

  const notificationMock = {
    showError: vi.fn(),
    showSuccess: vi.fn(),
  };

  const routerMock = {
    navigate: vi.fn(),
  };

  const cdrMock = {
    detectChanges: vi.fn(),
  };

  const routeMock = {
    paramMap: of(convertToParamMap({ id: '5' })),
    snapshot: { paramMap: convertToParamMap({ id: '5' }) },
  };

  const mockRequest = {
    id: 5,
    amount: 300,
    status: 0,
    purposeOfPayment: 'Engine repair',
    createdAt: '2026-01-01T00:00:00Z',
    statusHistory: [],
  } as unknown as PaymentRequestByTeamDto;

  const mockRequestWithUser = {
    id: 7,
    amount: 150.5,
    status: 0,
    dueDate: '2026-06-01T00:00:00Z',
    statusHistory: [],
    user: { id: 1, name: 'Jane Doe', email: 'jane@example.com' },
  } as unknown as PaymentRequestByTeamDto;

  beforeEach(async () => {
    vi.clearAllMocks();

    await TestBed.configureTestingModule({
      imports: [TeamRequestAdminDetailComponent],
      providers: [
        { provide: PaymentRequestByTeamService, useValue: serviceMock },
        { provide: NotificationService, useValue: notificationMock },
        { provide: ActivatedRoute, useValue: routeMock },
        { provide: Router, useValue: routerMock },
        { provide: ChangeDetectorRef, useValue: cdrMock },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(TeamRequestAdminDetailComponent);
    component = fixture.componentInstance;
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('should create', () => {
    serviceMock.getPaymentRequestsByTeamById.mockReturnValue(of(mockRequest));
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should call getPaymentRequestsByTeamById with the route id on init', () => {
    serviceMock.getPaymentRequestsByTeamById.mockReturnValue(of(mockRequest));
    component.ngOnInit();
    expect(serviceMock.getPaymentRequestsByTeamById).toHaveBeenCalledWith(5, expect.any(Object));
  });

  it('should call getPaymentRequestsByTeamById with all includes on init', () => {
    serviceMock.getPaymentRequestsByTeamById.mockReturnValue(of(mockRequest));
    component.ngOnInit();
    expect(serviceMock.getPaymentRequestsByTeamById).toHaveBeenCalledWith(5, {
      IncludeUser: true,
      IncludeTeam: true,
      IncludeStatusHistory: true,
    });
  });

  it('should set request and clear loading on successful load', () => {
    serviceMock.getPaymentRequestsByTeamById.mockReturnValue(of(mockRequest));
    component.ngOnInit();
    expect(component.request).toEqual(mockRequest);
    expect(component.loading).toBe(false);
  });

  it('should show error and clear loading when load fails', () => {
    serviceMock.getPaymentRequestsByTeamById.mockReturnValue(
      throwError(() => new Error('Not found')),
    );
    component.ngOnInit();
    expect(notificationMock.showError).toHaveBeenCalledWith(
      'Could not load payment request: Not found',
    );
    expect(component.loading).toBe(false);
  });

  it('should navigate to /payment-requests-by-team on back', () => {
    component.onBack();
    expect(routerMock.navigate).toHaveBeenCalledWith(['/payment-requests-by-team']);
  });

  describe('openEmailModal / openSlackModal', () => {
    it('sets modalType to email when openEmailModal is called', () => {
      component.openEmailModal();
      expect(component.modalType).toBe('email');
    });

    it('sets modalType to slack when openSlackModal is called', () => {
      component.openSlackModal();
      expect(component.modalType).toBe('slack');
    });
  });

  describe('notificationEmail', () => {
    it('returns the user email from the loaded request', () => {
      component.request = mockRequestWithUser;
      expect(component.notificationEmail).toBe('jane@example.com');
    });

    it('returns empty string when request has no user', () => {
      component.request = mockRequest;
      expect(component.notificationEmail).toBe('');
    });
  });

  describe('notificationSubject', () => {
    it('returns a subject line containing the request id', () => {
      component.request = mockRequestWithUser;
      expect(component.notificationSubject).toContain('7');
    });
  });

  describe('notificationMessage', () => {
    it('returns empty string when request is null', () => {
      component.request = null;
      expect(component.notificationMessage).toBe('');
    });

    it('returns an email message containing name, id, and amount when modalType is email', () => {
      component.request = mockRequestWithUser;
      component.modalType = 'email';
      const message = component.notificationMessage;
      expect(message).toContain('Jane Doe');
      expect(message).toContain('7');
      expect(message).toContain('150');
    });

    it('returns a slack message containing id and amount when modalType is slack', () => {
      component.request = mockRequestWithUser;
      component.modalType = 'slack';
      const message = component.notificationMessage;
      expect(message).toContain('7');
      expect(message).toContain('150');
    });

    it('uses User as fallback name when the user is not included in the request', () => {
      component.request = mockRequest;
      component.modalType = 'email';
      expect(component.notificationMessage).toContain('User');
    });

    it('shows N/A for dueDate in email when dueDate is absent', () => {
      component.request = mockRequest;
      component.modalType = 'email';
      expect(component.notificationMessage).toContain('N/A');
    });
  });

  describe('canMarkAsPaid', () => {
    it('returns false when request is null', () => {
      component.request = null;
      expect(component.canMarkAsPaid).toBe(false);
    });

    it('returns false when status is Paid', () => {
      component.request = {
        ...mockRequest,
        status: TransactionStatus.Paid,
      } as unknown as PaymentRequestByTeamDto;
      expect(component.canMarkAsPaid).toBe(false);
    });

    it('returns false when status is Declined', () => {
      component.request = {
        ...mockRequest,
        status: TransactionStatus.Declined,
      } as unknown as PaymentRequestByTeamDto;
      expect(component.canMarkAsPaid).toBe(false);
    });

    it('returns true when status is Submitted', () => {
      component.request = {
        ...mockRequest,
        status: TransactionStatus.Submitted,
      } as unknown as PaymentRequestByTeamDto;
      expect(component.canMarkAsPaid).toBe(true);
    });

    it('returns true when status is ChangesRequested', () => {
      component.request = {
        ...mockRequest,
        status: TransactionStatus.ChangesRequested,
      } as unknown as PaymentRequestByTeamDto;
      expect(component.canMarkAsPaid).toBe(true);
    });

    it('returns true when status is Approved', () => {
      component.request = {
        ...mockRequest,
        status: TransactionStatus.Approved,
      } as unknown as PaymentRequestByTeamDto;
      expect(component.canMarkAsPaid).toBe(true);
    });
  });

  describe('openMarkAsPaidModal / cancelMarkAsPaid', () => {
    it('sets showMarkAsPaidModal to true and resets comment to default on open', () => {
      component.markAsPaidComment = 'some old comment';
      component.openMarkAsPaidModal();
      expect(component.showMarkAsPaidModal).toBe(true);
      expect(component.markAsPaidComment).toBe('Payment manually approved and processed.');
    });

    it('sets showMarkAsPaidModal to false on cancel', () => {
      component.showMarkAsPaidModal = true;
      component.cancelMarkAsPaid();
      expect(component.showMarkAsPaidModal).toBe(false);
    });
  });

  describe('confirmMarkAsPaid', () => {
    it('does nothing when request is null', () => {
      component.request = null;
      component.confirmMarkAsPaid();
      expect(serviceMock.markAsPaid).not.toHaveBeenCalled();
    });

    it('calls markAsPaid with request id and comment, shows success, clears modal and updates request on success', () => {
      const updatedRequest = {
        ...mockRequest,
        status: TransactionStatus.Paid,
      } as unknown as PaymentRequestByTeamDto;
      component.request = mockRequest;
      component.markAsPaidComment = 'manually processed';
      serviceMock.markAsPaid.mockReturnValue(of(updatedRequest));

      component.confirmMarkAsPaid();

      expect(serviceMock.markAsPaid).toHaveBeenCalledWith(5, { comment: 'manually processed' });
      expect(notificationMock.showSuccess).toHaveBeenCalledWith('Payment marked as paid.');
      expect(component.markAsPaidLoading).toBe(false);
      expect(component.showMarkAsPaidModal).toBe(false);
      expect(component.request?.status).toBe(TransactionStatus.Paid);
      expect(serviceMock.getPaymentRequestsByTeamById).not.toHaveBeenCalled();
    });

    it('shows error notification and clears loading but keeps modal open on failure', () => {
      component.request = mockRequest;
      component.showMarkAsPaidModal = true;
      serviceMock.markAsPaid.mockReturnValue(throwError(() => new Error('server error')));

      component.confirmMarkAsPaid();

      expect(notificationMock.showError).toHaveBeenCalledWith(
        'Could not mark as paid: server error',
      );
      expect(component.markAsPaidLoading).toBe(false);
      expect(component.showMarkAsPaidModal).toBe(true);
    });
  });

  describe('canDelete', () => {
    it('returns true when status is Submitted', () => {
      component.request = {
        ...mockRequest,
        status: TransactionStatus.Submitted,
      } as unknown as PaymentRequestByTeamDto;
      expect(component.canDelete).toBe(true);
    });

    it('returns false when status is Approved', () => {
      component.request = {
        ...mockRequest,
        status: TransactionStatus.Approved,
      } as unknown as PaymentRequestByTeamDto;
      expect(component.canDelete).toBe(false);
    });

    it('returns false when request is null', () => {
      component.request = null;
      expect(component.canDelete).toBe(false);
    });
  });

  describe('openDeleteModal / cancelDelete', () => {
    it('resets reason and shows modal on open', () => {
      component.deleteReason = 'old reason';
      component.openDeleteModal();
      expect(component.showDeleteModal).toBe(true);
      expect(component.deleteReason).toBe('');
    });

    it('hides modal on cancel', () => {
      component.showDeleteModal = true;
      component.cancelDelete();
      expect(component.showDeleteModal).toBe(false);
    });
  });

  describe('confirmDelete', () => {
    it('does nothing when request is null', () => {
      component.request = null;
      component.confirmDelete();
      expect(serviceMock.deletePaymentRequestByTeam).not.toHaveBeenCalled();
    });

    it('calls service with id and reason, shows success and navigates on success', () => {
      component.request = mockRequest;
      component.deleteReason = 'Budget cut';
      serviceMock.deletePaymentRequestByTeam.mockReturnValue(of(undefined));

      component.confirmDelete();

      expect(serviceMock.deletePaymentRequestByTeam).toHaveBeenCalledWith(5, 'Budget cut');
      expect(notificationMock.showSuccess).toHaveBeenCalledWith('Payment request deleted.');
      expect(routerMock.navigate).toHaveBeenCalledWith(['/payment-requests-by-team']);
    });

    it('passes null as reason when deleteReason is empty', () => {
      component.request = mockRequest;
      component.deleteReason = '';
      serviceMock.deletePaymentRequestByTeam.mockReturnValue(of(undefined));

      component.confirmDelete();

      expect(serviceMock.deletePaymentRequestByTeam).toHaveBeenCalledWith(5, null);
    });

    it('shows error and clears loading on failure', () => {
      component.request = mockRequest;
      component.deleteReason = '';
      serviceMock.deletePaymentRequestByTeam.mockReturnValue(
        throwError(() => new Error('server error')),
      );

      component.confirmDelete();

      expect(notificationMock.showError).toHaveBeenCalledWith(
        'Could not delete payment request: server error',
      );
      expect(component.deleteLoading).toBe(false);
    });
  });
});
