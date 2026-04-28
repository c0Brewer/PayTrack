import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormsModule } from '@angular/forms';
import { vi } from 'vitest';

import { GetTeamOptions } from '../../../types/exporter';

import { TeamFilterComponent } from './team-filter-component';

describe('TeamFilterComponent', () => {
  let component: TeamFilterComponent;
  let fixture: ComponentFixture<TeamFilterComponent>;

  beforeEach(async () => {
    vi.useFakeTimers();

    await TestBed.configureTestingModule({
      imports: [TeamFilterComponent, FormsModule],
    }).compileComponents();

    fixture = TestBed.createComponent(TeamFilterComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should emit an updated filter when the team name changes after debounce', () => {
    const spy = vi.spyOn(component.updateFilter, 'emit');

    // Fake timers keep the debounce-based tests deterministic and fast.
    component.onNameFilterChange({ target: { value: 'Platform' } } as unknown as Event);
    vi.advanceTimersByTime(400);

    expect(spy).toHaveBeenCalledWith(expect.objectContaining({ Name: 'Platform' }));
  });

  it('should emit an updated filter when the description changes after debounce', () => {
    const spy = vi.spyOn(component.updateFilter, 'emit');

    component.onDescriptionFilterChange({ target: { value: 'Core systems' } } as unknown as Event);
    vi.advanceTimersByTime(400);

    expect(spy).toHaveBeenCalledWith(expect.objectContaining({ Description: 'Core systems' }));
  });

  it('should convert the minimum budget input into a number before emitting', () => {
    const spy = vi.spyOn(component.updateFilter, 'emit');

    component.onMinBudgetFilterChange({ target: { value: '250' } } as unknown as Event);
    vi.advanceTimersByTime(100);

    expect(spy).toHaveBeenCalledWith(expect.objectContaining({ MinBudget: 250 }));
  });

  it('should convert the maximum budget input into a number before emitting', () => {
    const spy = vi.spyOn(component.updateFilter, 'emit');

    component.onMaxBudgetFilterChange({ target: { value: '900' } } as unknown as Event);
    vi.advanceTimersByTime(100);

    expect(spy).toHaveBeenCalledWith(expect.objectContaining({ MaxBudget: 900 }));
  });

  it('should emit the new limit when the page size changes', () => {
    const spy = vi.spyOn(component.limitChange, 'emit');
    component.limit = 25;

    component.onLimitChange();

    expect(spy).toHaveBeenCalledWith(25);
  });

  it('getGetTeamOptions should build the team query object used by the parent component', () => {
    component.filterName = 'Platform';
    component.filterDescription = 'Core systems';
    component.filterMinBudget = 100;
    component.filterMaxBudget = 900;

    const options: GetTeamOptions = component.getGetTeamOptions();

    expect(options).toEqual({
      Name: 'Platform',
      Description: 'Core systems',
      MinBudget: 100,
      MaxBudget: 900,
      Limit: undefined,
      Offset: undefined,
    });
  });
});
