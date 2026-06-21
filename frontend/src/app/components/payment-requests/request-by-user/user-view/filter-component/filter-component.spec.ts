import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';

import { TeamService } from '../../../../../services/team/team-service';

import { UserInvoiceFilterComponent } from './filter-component';

describe('UserInvoiceFilterComponent', () => {
  let component: UserInvoiceFilterComponent;
  let fixture: ComponentFixture<UserInvoiceFilterComponent>;

  const teamServiceMock = {
    getTeams: vi.fn().mockReturnValue(of({ items: [], totalCount: 0, limit: 1000, offset: 0 })),
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UserInvoiceFilterComponent],
      providers: [{ provide: TeamService, useValue: teamServiceMock }],
    }).compileComponents();

    fixture = TestBed.createComponent(UserInvoiceFilterComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load teams on init', () => {
    expect(teamServiceMock.getTeams).toHaveBeenCalled();
    expect(component.teams).toEqual([]);
  });

  it('should emit filter options via updateFilter output', () => {
    let emitted: unknown;
    component.updateFilter.subscribe((f) => (emitted = f));

    component.emitFilter();

    expect(emitted).toBeDefined();
  });

  it('should include status in filter options when filterStatus is set', () => {
    component.filterStatus = 2;

    const options = component.getFilterOptions();

    expect(options!.Status as number).toBe(2);
  });

  it('should apply initial filter options to the visible filter state', () => {
    component.initialFilterOptions = {
      InvoiceNumber: 'INV-42',
      Status: 2,
      PurposeOfPayment: 'Hardware',
      TeamId: 7,
      PayoutType: 1,
      MinAmount: 10,
      MaxAmount: 25,
      MinCreatedAt: '2026-01-01',
      MaxCreatedAt: '2026-01-31',
      MinPaidAt: '2026-02-01',
      MaxPaidAt: '2026-02-28',
    };

    component.ngOnChanges({
      initialFilterOptions: {
        currentValue: component.initialFilterOptions,
        previousValue: null,
        firstChange: false,
        isFirstChange: () => false,
      },
    });

    expect(component.filterInvoiceNumber).toBe('INV-42');
    expect(component.filterStatus).toBe(2);
    expect(component.filterPurpose).toBe('Hardware');
    expect(component.filterTeamId).toBe(7);
    expect(component.filterPayoutType).toBe(1);
    expect(component.filterMinAmount).toBe('10');
    expect(component.filterMaxAmount).toBe('25');
    expect(component.filterMinCreatedAt).toBe('2026-01-01');
    expect(component.filterMaxCreatedAt).toBe('2026-01-31');
    expect(component.filterMinPaidAt).toBe('2026-02-01');
    expect(component.filterMaxPaidAt).toBe('2026-02-28');
  });
});
