import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TransactionStatus } from '../../../../../types/exporter';

import { TeamRequestTeamFilterComponent } from './filter-component';

describe('TeamRequestTeamFilterComponent', () => {
  let component: TeamRequestTeamFilterComponent;
  let fixture: ComponentFixture<TeamRequestTeamFilterComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TeamRequestTeamFilterComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(TeamRequestTeamFilterComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should emit filter options when emitFilter is called', () => {
    fixture.detectChanges();
    let emitted: unknown;
    component.updateFilter.subscribe((filter) => (emitted = filter));

    component.emitFilter();

    expect(emitted).toBeDefined();
  });

  it('should include status in filter options when filterStatus is set', () => {
    component.filterStatus = TransactionStatus.Submitted;

    const options = component.getFilterOptions()!;

    expect(options.Status).toBe(TransactionStatus.Submitted);
  });

  it('should return undefined for unset numeric filter fields', () => {
    component.filterMinAmount = '';
    component.filterMaxAmount = '';

    const options = component.getFilterOptions()!;

    expect(options.MinAmount).toBeUndefined();
    expect(options.MaxAmount).toBeUndefined();
  });

  it('should return numeric values for set amount filter fields', () => {
    component.filterMinAmount = '50';
    component.filterMaxAmount = '200';

    const options = component.getFilterOptions()!;

    expect(options.MinAmount).toBe(50);
    expect(options.MaxAmount).toBe(200);
  });

  it('should include due date filters when set', () => {
    component.filterMinDueDate = '2026-01-01';
    component.filterMaxDueDate = '2026-01-31';

    const options = component.getFilterOptions()!;

    expect(options.MinDueDate).toBe('2026-01-01');
    expect(options.MaxDueDate).toBe('2026-01-31');
  });

  it('should not include admin-only filters', () => {
    const options = component.getFilterOptions()!;

    expect(options.TeamId).toBeUndefined();
    expect(options.UserId).toBeUndefined();
  });

  it('should emit limitChange when onLimitChange is called', () => {
    fixture.detectChanges();
    let emittedLimit: number | undefined;
    component.limitChange.subscribe((limit) => (emittedLimit = limit));
    component.limit = 25;

    component.onLimitChange();

    expect(emittedLimit).toBe(25);
  });
});
