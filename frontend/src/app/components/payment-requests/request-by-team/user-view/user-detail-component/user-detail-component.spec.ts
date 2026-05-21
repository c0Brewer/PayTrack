import { ChangeDetectorRef } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, Router } from '@angular/router';
import { of, throwError } from 'rxjs';

import { NotificationService } from '../../../../../services/notification/notification-service';
import { PaymentRequestByTeamService } from '../../../../../services/payment-request-by-team/payment-request-by-team-service';
import { PaymentRequestByTeamDto } from '../../../../../types/exporter';

import { TeamRequestUserDetailComponent } from './user-detail-component';

describe('TeamRequestUserDetailComponent', () => {
  let component: TeamRequestUserDetailComponent;
  let fixture: ComponentFixture<TeamRequestUserDetailComponent>;

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
  } as unknown as PaymentRequestByTeamDto;

  beforeEach(async () => {
    vi.clearAllMocks();

    await TestBed.configureTestingModule({
      imports: [TeamRequestUserDetailComponent],
      providers: [
        { provide: PaymentRequestByTeamService, useValue: serviceMock },
        { provide: NotificationService, useValue: notificationMock },
        { provide: ActivatedRoute, useValue: routeMock },
        { provide: Router, useValue: routerMock },
        { provide: ChangeDetectorRef, useValue: cdrMock },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(TeamRequestUserDetailComponent);
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

  it('should fetch without optional includes on init', () => {
    serviceMock.getPaymentRequestsByTeamById.mockReturnValue(of(mockRequest));
    component.ngOnInit();
    expect(serviceMock.getPaymentRequestsByTeamById).toHaveBeenCalledWith(5, {
      IncludeUser: false,
      IncludeTeam: false,
      IncludeStatusHistory: false,
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

  it('should navigate to /my-team-requests on back', () => {
    component.onBack();
    expect(routerMock.navigate).toHaveBeenCalledWith(['/my-team-requests']);
  });
});
