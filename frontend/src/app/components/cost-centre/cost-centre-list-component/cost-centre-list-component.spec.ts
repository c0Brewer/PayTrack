import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { vi } from 'vitest';

import { BudgetDto, CostCentreDto } from '../../../types/exporter';

import { CostCentreListComponent } from './cost-centre-list-component';

const now = new Date();
const activeBudget: BudgetDto = {
  id: 10,
  teamId: 1,
  costCentreId: 1,
  targetAmount: 500,
  periodStart: new Date(now.getFullYear() - 1, 0, 1).toISOString(),
  periodEnd: new Date(now.getFullYear() + 1, 11, 31).toISOString(),
};
const expiredBudget: BudgetDto = {
  id: 11,
  teamId: 1,
  costCentreId: 1,
  targetAmount: 500,
  periodStart: '2020-01-01T00:00:00Z',
  periodEnd: '2020-12-31T00:00:00Z',
};

const mockCostCentres: CostCentreDto[] = [
  {
    id: 1,
    name: 'Aerodynamics',
    description: 'Aero costs',
    displayColor: '#FF5733',
    budgets: [],
    isActive: true,
  },
  {
    id: 2,
    name: 'Powertrain',
    description: null,
    displayColor: null,
    budgets: [],
    isActive: false,
  },
];

describe('CostCentreListComponent', () => {
  let component: CostCentreListComponent;
  let fixture: ComponentFixture<CostCentreListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CostCentreListComponent],
      providers: [provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(CostCentreListComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should emit openEdit when onOpenEdit is called', () => {
    const spy = vi.spyOn(component.openEdit, 'emit');
    component.onOpenEdit(mockCostCentres[0]);
    expect(spy).toHaveBeenCalledOnce();
    expect(spy).toHaveBeenCalledWith(mockCostCentres[0]);
  });

  it('should emit openDelete when onOpenDelete is called', () => {
    const spy = vi.spyOn(component.openDelete, 'emit');
    component.onOpenDelete(mockCostCentres[1]);
    expect(spy).toHaveBeenCalledOnce();
    expect(spy).toHaveBeenCalledWith(mockCostCentres[1]);
  });

  it('should default costCentres to empty array', () => {
    expect(component.costCentres).toEqual([]);
  });

  it('should render a row for each cost centre', () => {
    component.costCentres = mockCostCentres;
    fixture.detectChanges();
    const rows = fixture.nativeElement.querySelectorAll('tbody tr');
    expect(rows.length).toBe(2);
  });

  it('should show "No current budget" when a cost centre has no budgets', () => {
    component.costCentres = [
      { id: 1, name: 'A', description: null, displayColor: null, budgets: [], isActive: true },
    ];
    fixture.detectChanges();
    const span = fixture.nativeElement.querySelector('.no-budgets');
    expect(span).not.toBeNull();
    expect(span.textContent).toContain('No current budget');
  });

  it('should show "No current budget" when all budgets are expired', () => {
    component.costCentres = [
      {
        id: 1,
        name: 'A',
        description: null,
        displayColor: null,
        budgets: [expiredBudget],
        isActive: true,
      },
    ];
    fixture.detectChanges();
    const span = fixture.nativeElement.querySelector('.no-budgets');
    expect(span).not.toBeNull();
    expect(span.textContent).toContain('No current budget');
  });

  it('should show the active budget amount when a current budget exists', () => {
    component.costCentres = [
      {
        id: 1,
        name: 'A',
        description: null,
        displayColor: null,
        budgets: [activeBudget],
        isActive: true,
      },
    ];
    fixture.detectChanges();
    const span = fixture.nativeElement.querySelector('.has-budgets');
    expect(span).not.toBeNull();
    expect(span.textContent).toContain('500');
  });

  describe('status badge', () => {
    it('should show "Active" badge for an active cost centre', () => {
      component.costCentres = mockCostCentres;
      fixture.detectChanges();
      const badges = fixture.nativeElement.querySelectorAll('.status-badge');
      expect(badges[0].textContent.trim()).toBe('Active');
      expect(badges[0].classList.contains('active')).toBe(true);
    });

    it('should show "Inactive" badge for an inactive cost centre', () => {
      component.costCentres = mockCostCentres;
      fixture.detectChanges();
      const badges = fixture.nativeElement.querySelectorAll('.status-badge');
      expect(badges[1].textContent.trim()).toBe('Inactive');
      expect(badges[1].classList.contains('active')).toBe(false);
    });
  });

  describe('getActiveBudget', () => {
    it('should return undefined for null or empty budgets', () => {
      expect(component.getActiveBudget(null)).toBeUndefined();
      expect(component.getActiveBudget(undefined)).toBeUndefined();
      expect(component.getActiveBudget([])).toBeUndefined();
    });

    it('should return undefined for an expired budget', () => {
      expect(component.getActiveBudget([expiredBudget])).toBeUndefined();
    });

    it('should return the active budget', () => {
      expect(component.getActiveBudget([activeBudget])).toEqual(activeBudget);
    });

    it('should return only the active budget when mixed with an expired one', () => {
      expect(component.getActiveBudget([expiredBudget, activeBudget])).toEqual(activeBudget);
    });
  });
});
