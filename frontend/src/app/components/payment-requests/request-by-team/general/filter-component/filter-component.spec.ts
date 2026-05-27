import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';

import { CostCentreService } from '../../../../../services/cost-centre/cost-centre-service';
import { TeamService } from '../../../../../services/team/team-service';
import { UserService } from '../../../../../services/user/user-service';

import { TeamRequestFilterComponent } from './filter-component';

describe('TeamRequestFilterComponent', () => {
  let component: TeamRequestFilterComponent;
  let fixture: ComponentFixture<TeamRequestFilterComponent>;

  const teamServiceMock = {
    getTeams: vi.fn().mockReturnValue(of({ items: [{ id: 1, name: 'Team A' }], totalCount: 1 })),
  };

  const costCentreServiceMock = {
    getCostCentres: vi
      .fn()
      .mockReturnValue(of({ items: [{ id: 1, name: 'CC-1' }], totalCount: 1 })),
  };

  const userServiceMock = {
    getUser: vi.fn().mockReturnValue(of({ items: [{ id: 1, name: 'Alice' }], totalCount: 1 })),
  };

  beforeEach(async () => {
    vi.clearAllMocks();

    await TestBed.configureTestingModule({
      imports: [TeamRequestFilterComponent],
      providers: [
        { provide: TeamService, useValue: teamServiceMock },
        { provide: CostCentreService, useValue: costCentreServiceMock },
        { provide: UserService, useValue: userServiceMock },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(TeamRequestFilterComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should load teams on init', () => {
    fixture.detectChanges();
    expect(teamServiceMock.getTeams).toHaveBeenCalled();
    expect(component.teams).toHaveLength(1);
  });

  it('should load cost centres when showCostCentreFilter is true', () => {
    component.showCostCentreFilter = true;
    fixture.detectChanges();
    expect(costCentreServiceMock.getCostCentres).toHaveBeenCalled();
    expect(component.costCentres).toHaveLength(1);
  });

  it('should not load cost centres when showCostCentreFilter is false', () => {
    component.showCostCentreFilter = false;
    fixture.detectChanges();
    expect(costCentreServiceMock.getCostCentres).not.toHaveBeenCalled();
  });

  it('should load users when showUserFilter is true', () => {
    component.showUserFilter = true;
    fixture.detectChanges();
    expect(userServiceMock.getUser).toHaveBeenCalled();
    expect(component.users).toHaveLength(1);
  });

  it('should not load users when showUserFilter is false', () => {
    component.showUserFilter = false;
    fixture.detectChanges();
    expect(userServiceMock.getUser).not.toHaveBeenCalled();
  });

  it('should emit filter options when emitFilter is called', () => {
    fixture.detectChanges();
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

  it('should include teamId in filter options when filterTeamId is set', () => {
    component.filterTeamId = 3;
    const options = component.getFilterOptions();
    expect(options!.TeamId).toBe(3);
  });

  it('should return undefined for unset numeric filter fields', () => {
    component.filterMinAmount = '';
    component.filterMaxAmount = '';
    const options = component.getFilterOptions();
    expect(options!.MinAmount).toBeUndefined();
    expect(options!.MaxAmount).toBeUndefined();
  });

  it('should return numeric values for set amount filter fields', () => {
    component.filterMinAmount = '50';
    component.filterMaxAmount = '200';
    const options = component.getFilterOptions();
    expect(options!.MinAmount).toBe(50);
    expect(options!.MaxAmount).toBe(200);
  });

  it('should emit limitChange when onLimitChange is called', () => {
    fixture.detectChanges();
    let emittedLimit: number | undefined;
    component.limitChange.subscribe((l) => (emittedLimit = l));
    component.limit = 25;
    component.onLimitChange();
    expect(emittedLimit).toBe(25);
  });
});
