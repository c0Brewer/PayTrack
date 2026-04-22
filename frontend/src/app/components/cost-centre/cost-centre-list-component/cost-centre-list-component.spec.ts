import { SimpleChange } from '@angular/core';
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
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should populate dataSource.data when costCentres input changes', () => {
    component.costCentres = mockCostCentres;
    component.ngOnChanges({
      costCentres: new SimpleChange([], mockCostCentres, false),
    });
    expect(component.dataSource.data).toEqual(mockCostCentres);
  });

  it('should clear dataSource.data when costCentres is set to empty', () => {
    component.costCentres = [];
    component.ngOnChanges({
      costCentres: new SimpleChange(mockCostCentres, [], false),
    });
    expect(component.dataSource.data).toEqual([]);
  });

  it('should set dataSource.filter trimmed and lowercased', () => {
    const event = { target: { value: '  AERO  ' } } as unknown as Event;
    component.applyFilter(event);
    expect(component.dataSource.filter).toBe('aero');
  });

  it('should set dataSource.filter to empty string when input is whitespace', () => {
    const event = { target: { value: '   ' } } as unknown as Event;
    component.applyFilter(event);
    expect(component.dataSource.filter).toBe('');
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

  describe('sortingDataAccessor', () => {
    it('should return budgets count for budgets property', () => {
      const item: CostCentreDto = {
        id: 1,
        name: 'Test',
        description: 'Desc',
        displayColor: '#000',
        budgets: [{} as never, {} as never],
      };
      expect(component.dataSource.sortingDataAccessor(item, 'budgets')).toBe(2);
    });

    it('should return string value for string property', () => {
      expect(component.dataSource.sortingDataAccessor(mockCostCentres[0], 'name')).toBe(
        'Aerodynamics',
      );
    });

    it('should return number value for number property', () => {
      expect(component.dataSource.sortingDataAccessor(mockCostCentres[0], 'id')).toBe(1);
    });

    it('should return empty string for null property', () => {
      expect(component.dataSource.sortingDataAccessor(mockCostCentres[1], 'description')).toBe('');
    });
  });
});
