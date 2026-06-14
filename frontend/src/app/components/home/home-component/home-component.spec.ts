import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { vi } from 'vitest';

import { HomeDashboardService } from '../../../services/home/home-dashboard-service';
import { NotificationService } from '../../../services/notification/notification-service';
import { Role } from '../../../types/exporter';

import { HomeComponent } from './home-component';

describe('HomeComponent', () => {
  let component: HomeComponent;
  let fixture: ComponentFixture<HomeComponent>;

  const homeDashboardServiceMock = {
    getHomeDashboard: vi.fn().mockReturnValue(
      of({
        user: { id: 1, name: 'Alex', role: Role.REGULAR_USER },
        invoices: {
          openCount: 0,
          submittedCount: 0,
          paidCount: 0,
          openAmount: 0,
          lastPaidAt: null,
          recent: [],
        },
        paymentRequests: {
          openCount: 0,
          submittedCount: 0,
          paidCount: 0,
          openAmount: 0,
          lastPaidAt: null,
          recent: [],
        },
        actions: {
          missingBankAccount: false,
          bankInformationSkipped: false,
          needsAttentionCount: 0,
        },
      }),
    ),
  };

  const notificationServiceMock = {
    showError: vi.fn(),
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [HomeComponent],
      providers: [
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
});
