import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { vi } from 'vitest';

import { CostCentreDto } from '../../../types/exporter';

import { CostCentreListComponent } from './cost-centre-list-component';

const mockCostCentres: CostCentreDto[] = [
  { id: 1, name: 'Aerodynamics', description: 'Aero costs', displayColor: '#FF5733', budgets: [] },
  { id: 2, name: 'Powertrain', description: null, displayColor: null, budgets: [] },
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

  it('should show "no budgets" when a cost centre has no budgets', () => {
    component.costCentres = [{ id: 1, name: 'A', description: null, displayColor: null, budgets: [] }];
    fixture.detectChanges();
    const span = fixture.nativeElement.querySelector('.no-budgets');
    expect(span).not.toBeNull();
    expect(span.textContent).toContain('no budgets');
  });

  it('should show "has budgets" when a cost centre has budgets', () => {
    component.costCentres = [
      { id: 1, name: 'A', description: null, displayColor: null, budgets: [{ id: 10, teamId: 1, costCentreId: 1, targetAmount: 500, periodStart: '2024-01-01', periodEnd: '2024-12-31' }] },
    ];
    fixture.detectChanges();
    const span = fixture.nativeElement.querySelector('.has-budgets');
    expect(span).not.toBeNull();
  });
});
