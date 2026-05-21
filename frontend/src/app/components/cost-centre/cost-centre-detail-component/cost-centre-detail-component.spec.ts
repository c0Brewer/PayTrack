import { ChangeDetectorRef } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, Router } from '@angular/router';
import { of, Subject, throwError } from 'rxjs';
import { vi } from 'vitest';

import { CostCentreService } from '../../../services/cost-centre/cost-centre-service';
import { NotificationService } from '../../../services/notification/notification-service';
import { BudgetDto, CostCentreDto } from '../../../types/exporter';

import { CostCentreDetailComponent } from './cost-centre-detail-component';

const mockBudget: BudgetDto = {
  id: 1,
  teamId: 2,
  costCentreId: 10,
  targetAmount: 1500,
  periodStart: '2024-01-01T00:00:00Z',
  periodEnd: '2024-12-31T00:00:00Z',
};

const mockCostCentre: CostCentreDto = {
  id: 10,
  name: 'Aerodynamics',
  description: 'Aero costs',
  displayColor: '#FF5733',
  budgets: [mockBudget],
  isActive: true,
};

describe('CostCentreDetailComponent', () => {
  let component: CostCentreDetailComponent;
  let fixture: ComponentFixture<CostCentreDetailComponent>;
  let costCentreServiceMock: { getCostCentre: ReturnType<typeof vi.fn> };
  let notificationServiceMock: { showError: ReturnType<typeof vi.fn> };
  let routerMock: { navigate: ReturnType<typeof vi.fn> };
  let cdrMock: { detectChanges: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    costCentreServiceMock = {
      getCostCentre: vi.fn().mockReturnValue(of(mockCostCentre)),
    };
    notificationServiceMock = {
      showError: vi.fn(),
    };
    routerMock = {
      navigate: vi.fn(),
    };
    cdrMock = {
      detectChanges: vi.fn(),
    };

    await TestBed.configureTestingModule({
      imports: [CostCentreDetailComponent],
      providers: [
        { provide: CostCentreService, useValue: costCentreServiceMock },
        { provide: NotificationService, useValue: notificationServiceMock },
        { provide: Router, useValue: routerMock },
        { provide: ChangeDetectorRef, useValue: cdrMock },
        {
          provide: ActivatedRoute,
          useValue: { paramMap: of(convertToParamMap({ id: '10' })) },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(CostCentreDetailComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should default costCentre to null', () => {
    expect(component.costCentre).toBeNull();
  });

  describe('ngOnInit', () => {
    it('should call getCostCentre with the id parsed from route params', () => {
      fixture.detectChanges();
      expect(costCentreServiceMock.getCostCentre).toHaveBeenCalledWith(10);
    });

    it('should set costCentre on successful load', () => {
      fixture.detectChanges();
      expect(component.costCentre).toEqual(mockCostCentre);
    });

    it('should show error notification when getCostCentre fails', () => {
      costCentreServiceMock.getCostCentre.mockReturnValueOnce(
        throwError(() => new Error('Not found')),
      );
      fixture.detectChanges();
      expect(notificationServiceMock.showError).toHaveBeenCalledWith(
        'Could not load cost centre: Not found',
      );
    });

    it('should leave costCentre null when getCostCentre fails', () => {
      costCentreServiceMock.getCostCentre.mockReturnValueOnce(
        throwError(() => new Error('Not found')),
      );
      fixture.detectChanges();
      expect(component.costCentre).toBeNull();
    });
  });

  describe('goBack', () => {
    it('should navigate to /cost-centre', () => {
      component.goBack();
      expect(routerMock.navigate).toHaveBeenCalledWith(['/cost-centre']);
    });
  });

  describe('template', () => {
    it('should show "Loading..." when costCentre is null', () => {
      const subject = new Subject<CostCentreDto>();
      costCentreServiceMock.getCostCentre.mockReturnValue(subject.asObservable());
      fixture.detectChanges();
      expect(fixture.nativeElement.querySelector('p')?.textContent).toContain('Loading...');
    });

    it('should show the back button', () => {
      fixture.detectChanges();
      expect(fixture.nativeElement.querySelector('.detail-shell__back')).not.toBeNull();
    });

    it('should show cost centre name in heading', () => {
      fixture.detectChanges();
      expect(fixture.nativeElement.querySelector('h1')?.textContent).toContain('Aerodynamics');
    });

    it('should show cost centre description', () => {
      fixture.detectChanges();
      expect(fixture.nativeElement.textContent).toContain('Aero costs');
    });

    it('should show "—" when description is null', () => {
      costCentreServiceMock.getCostCentre.mockReturnValue(
        of({ ...mockCostCentre, description: null }),
      );
      fixture.detectChanges();
      expect(fixture.nativeElement.textContent).toContain('—');
    });

    it('should show "No budgets assigned." when budgets array is empty', () => {
      costCentreServiceMock.getCostCentre.mockReturnValue(of({ ...mockCostCentre, budgets: [] }));
      fixture.detectChanges();
      expect(fixture.nativeElement.querySelector('.no-budgets')?.textContent).toContain(
        'No budgets assigned.',
      );
    });

    it('should show "No budgets assigned." when budgets is null', () => {
      costCentreServiceMock.getCostCentre.mockReturnValue(of({ ...mockCostCentre, budgets: null }));
      fixture.detectChanges();
      expect(fixture.nativeElement.querySelector('.no-budgets')?.textContent).toContain(
        'No budgets assigned.',
      );
    });

    it('should render a row for each budget', () => {
      fixture.detectChanges();
      const rows = fixture.nativeElement.querySelectorAll('.budget-table tbody tr');
      expect(rows.length).toBe(1);
    });

    it('should display the formatted budget target amount', () => {
      fixture.detectChanges();
      expect(fixture.nativeElement.textContent).toContain('1.500');
    });

    it('should display budget period dates sliced to YYYY-MM-DD', () => {
      fixture.detectChanges();
      expect(fixture.nativeElement.textContent).toContain('2024-01-01');
      expect(fixture.nativeElement.textContent).toContain('2024-12-31');
    });

    it('should show budget count in heading', () => {
      fixture.detectChanges();
      expect(fixture.nativeElement.querySelector('h3')?.textContent).toContain('1');
    });
  });
});
