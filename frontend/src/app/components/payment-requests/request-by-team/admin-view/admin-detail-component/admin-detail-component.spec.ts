import { ChangeDetectorRef } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, Router } from '@angular/router';
import { of, throwError } from 'rxjs';

import { NotificationService } from '../../../../../services/notification/notification-service';
import { PaymentRequestByTeamService } from '../../../../../services/payment-request-by-team/payment-request-by-team-service';
import { PaymentRequestByTeamDto } from '../../../../../types/exporter';

import { TeamRequestAdminDetailComponent } from './admin-detail-component';

describe('TeamRequestAdminDetailComponent', () => {
  let component: TeamRequestAdminDetailComponent;
  let fixture: ComponentFixture<TeamRequestAdminDetailComponent>;

  const serviceMock = {
    getPaymentRequestsByTeamById: vi.fn(),
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
  });
});
