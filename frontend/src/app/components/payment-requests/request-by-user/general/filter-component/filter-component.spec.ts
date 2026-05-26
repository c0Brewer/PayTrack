import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';

import { TeamService } from '../../../../../services/team/team-service';

import { InvoiceFilterComponent } from './filter-component';

describe('InvoiceFilterComponent', () => {
  let component: InvoiceFilterComponent;
  let fixture: ComponentFixture<InvoiceFilterComponent>;

  const teamServiceMock = {
    getTeams: vi.fn().mockReturnValue(of({ items: [], totalCount: 0, limit: 1000, offset: 0 })),
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [InvoiceFilterComponent],
      providers: [{ provide: TeamService, useValue: teamServiceMock }],
    }).compileComponents();

    fixture = TestBed.createComponent(InvoiceFilterComponent);
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
});
