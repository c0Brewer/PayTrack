import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormsModule } from '@angular/forms';
import { vi } from 'vitest';

import { CostCentreFilterComponent } from './cost-centre-filter-component';

describe('CostCentreFilterComponent', () => {
  let component: CostCentreFilterComponent;
  let fixture: ComponentFixture<CostCentreFilterComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CostCentreFilterComponent, FormsModule],
    }).compileComponents();

    fixture = TestBed.createComponent(CostCentreFilterComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    vi.useFakeTimers();
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('onNameFilterChange should emit updateFilter with Name after debounce', () => {
    const spy = vi.spyOn(component.updateFilter, 'emit');
    component.onNameFilterChange({ target: { value: 'Aero' } } as unknown as Event);
    vi.advanceTimersByTime(400);
    expect(spy).toHaveBeenCalledWith(expect.objectContaining({ Name: 'Aero' }));
  });

  it('onNameFilterChange should emit Name as undefined when value is empty', () => {
    const spy = vi.spyOn(component.updateFilter, 'emit');
    component.onNameFilterChange({ target: { value: '' } } as unknown as Event);
    vi.advanceTimersByTime(400);
    expect(spy).toHaveBeenCalledWith(expect.objectContaining({ Name: undefined }));
  });

  it('onDescriptionFilterChange should emit updateFilter with Description after debounce', () => {
    const spy = vi.spyOn(component.updateFilter, 'emit');
    component.onDescriptionFilterChange({ target: { value: 'Engine' } } as unknown as Event);
    vi.advanceTimersByTime(400);
    expect(spy).toHaveBeenCalledWith(expect.objectContaining({ Description: 'Engine' }));
  });

  it('onMinBudgetFilterChange should emit MinBudget as number after debounce', () => {
    const spy = vi.spyOn(component.updateFilter, 'emit');
    component.onMinBudgetFilterChange({ target: { value: '100' } } as unknown as Event);
    vi.advanceTimersByTime(400);
    expect(spy).toHaveBeenCalledWith(expect.objectContaining({ MinBudget: 100 }));
  });

  it('onMinBudgetFilterChange should emit MinBudget as undefined when input is empty', () => {
    const spy = vi.spyOn(component.updateFilter, 'emit');
    component.onMinBudgetFilterChange({ target: { value: '' } } as unknown as Event);
    vi.advanceTimersByTime(400);
    expect(spy).toHaveBeenCalledWith(expect.objectContaining({ MinBudget: undefined }));
  });

  it('onMaxBudgetFilterChange should emit MaxBudget as number after debounce', () => {
    const spy = vi.spyOn(component.updateFilter, 'emit');
    component.onMaxBudgetFilterChange({ target: { value: '500' } } as unknown as Event);
    vi.advanceTimersByTime(400);
    expect(spy).toHaveBeenCalledWith(expect.objectContaining({ MaxBudget: 500 }));
  });

  it('onMaxBudgetFilterChange should emit MaxBudget as undefined when input is empty', () => {
    const spy = vi.spyOn(component.updateFilter, 'emit');
    component.onMaxBudgetFilterChange({ target: { value: '' } } as unknown as Event);
    vi.advanceTimersByTime(400);
    expect(spy).toHaveBeenCalledWith(expect.objectContaining({ MaxBudget: undefined }));
  });

  it('onLimitChange should emit limitChange with the current limit value', () => {
    const spy = vi.spyOn(component.limitChange, 'emit');
    component.limit = 25;
    component.onLimitChange();
    expect(spy).toHaveBeenCalledWith(25);
  });

  it('getOptions should return correct options with all fields set', () => {
    component.filterName = 'Aero';
    component.filterDescription = 'Engine';
    component.filterMinBudget = 100;
    component.filterMaxBudget = 500;
    expect(component.getOptions()).toEqual({
      Name: 'Aero',
      Description: 'Engine',
      MinBudget: 100,
      MaxBudget: 500,
      Limit: undefined,
      Offset: undefined,
    });
  });

  it('getOptions should return undefined for all fields when filter fields are empty', () => {
    component.filterName = '';
    component.filterDescription = '';
    component.filterMinBudget = undefined;
    component.filterMaxBudget = undefined;
    expect(component.getOptions()).toEqual({
      Name: undefined,
      Description: undefined,
      MinBudget: undefined,
      MaxBudget: undefined,
      Limit: undefined,
      Offset: undefined,
    });
  });
});
