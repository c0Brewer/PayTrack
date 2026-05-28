import { ChangeDetectorRef } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, Router, provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';

import { CostCentreService } from '../../../services/cost-centre/cost-centre-service';
import { NotificationService } from '../../../services/notification/notification-service';
import { TeamService } from '../../../services/team/team-service';
import { CostCentreDtoPaginatedResponse, TeamDto } from '../../../types/exporter';

import { TeamDetailComponent } from './team-detail-component';

const mockTeam: TeamDto = {
  id: 1,
  name: 'Platform',
  description: 'Builds product features',
  displayColor: '#2563eb',
  members: [],
  budgets: [
    {
      id: 10,
      name: 'Vehicle budget',
      description: null,
      teamId: 1,
      costCentreId: 20,
      seasonId: 1,
      targetAmount: 5000,
      periodStart: '2026-01-01T00:00:00Z',
      periodEnd: '2026-12-31T00:00:00Z',
      transactionIds: [],
    },
  ],
};

const mockCostCentres: CostCentreDtoPaginatedResponse = {
  items: [
    {
      id: 20,
      name: 'Vehicle',
      description: null,
      displayColor: null,
      budgets: [],
      isActive: true,
    },
  ],
  totalCount: 1,
  limit: 1000,
  offset: 0,
  hasNext: false,
  hasPrevious: false,
};

describe('TeamDetailComponent', () => {
  let component: TeamDetailComponent;
  let fixture: ComponentFixture<TeamDetailComponent>;
  let teamServiceMock: { getTeamById: ReturnType<typeof vi.fn> };
  let costCentreServiceMock: { getCostCentres: ReturnType<typeof vi.fn> };
  let notificationServiceMock: { showError: ReturnType<typeof vi.fn> };
  let router: Router;
  let cdrMock: { detectChanges: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    teamServiceMock = {
      getTeamById: vi.fn().mockReturnValue(of(mockTeam)),
    };
    costCentreServiceMock = {
      getCostCentres: vi.fn().mockReturnValue(of(mockCostCentres)),
    };
    notificationServiceMock = {
      showError: vi.fn(),
    };
    cdrMock = {
      detectChanges: vi.fn(),
    };

    await TestBed.configureTestingModule({
      imports: [TeamDetailComponent],
      providers: [
        provideRouter([]),
        { provide: TeamService, useValue: teamServiceMock },
        { provide: CostCentreService, useValue: costCentreServiceMock },
        { provide: NotificationService, useValue: notificationServiceMock },
        { provide: ChangeDetectorRef, useValue: cdrMock },
        {
          provide: ActivatedRoute,
          useValue: { paramMap: of(convertToParamMap({ id: '1' })) },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(TeamDetailComponent);
    component = fixture.componentInstance;
    router = TestBed.inject(Router);
  });

  it('should load the team and cost centres', () => {
    fixture.detectChanges();

    expect(teamServiceMock.getTeamById).toHaveBeenCalledWith(1, {
      IncludeMembers: true,
      IncludeBudgets: true,
    });
    expect(costCentreServiceMock.getCostCentres).toHaveBeenCalledWith({ Limit: 1000, Offset: 0 });
    expect(component.team).toEqual(mockTeam);
    expect(component.costCentres).toEqual(mockCostCentres.items);
  });

  it('should render cost centre names as links in the budget table', () => {
    fixture.detectChanges();

    const costCentreLink = fixture.nativeElement.querySelector(
      '.budget-table tbody a',
    ) as HTMLAnchorElement;

    expect(costCentreLink.textContent?.trim()).toBe('Vehicle');
    expect(costCentreLink.getAttribute('href')).toBe('/cost-centre/20');
    expect(fixture.nativeElement.textContent).toContain('5.000');
  });

  it('should fall back to the cost centre id when the name is unknown', () => {
    component.costCentres = [];

    expect(component.getCostCentreName(99)).toBe('Cost Centre #99');
  });

  it('should show an error when cost centres cannot be loaded', () => {
    costCentreServiceMock.getCostCentres.mockReturnValueOnce(
      throwError(() => new Error('Cost centres unavailable')),
    );

    fixture.detectChanges();

    expect(notificationServiceMock.showError).toHaveBeenCalledWith(
      'Could not load cost centres: Cost centres unavailable',
    );
  });

  it('should navigate back to team management', () => {
    const navigateSpy = vi.spyOn(router, 'navigate');

    component.goBack();

    expect(navigateSpy).toHaveBeenCalledWith(['/team']);
  });
});
