import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';

import { TeamService } from '../../../../../services/team/team-service';
import { UserService } from '../../../../../services/user/user-service';
import { TransactionStatus } from '../../../../../types/exporter';

import { TeamRequestAdminFilterComponent } from './filter-component';

describe('TeamRequestAdminFilterComponent', () => {
  let component: TeamRequestAdminFilterComponent;
  let fixture: ComponentFixture<TeamRequestAdminFilterComponent>;

  const teamServiceMock = {
    getTeams: vi.fn(),
  };

  const userServiceMock = {
    getUser: vi.fn(),
  };

  beforeEach(async () => {
    vi.clearAllMocks();
    teamServiceMock.getTeams.mockReturnValue(
      of({ items: [{ id: 1, name: 'Team A' }], totalCount: 1 }),
    );
    userServiceMock.getUser.mockReturnValue(
      of({ items: [{ id: 2, name: 'Alice' }], totalCount: 1 }),
    );

    await TestBed.configureTestingModule({
      imports: [TeamRequestAdminFilterComponent],
      providers: [
        { provide: TeamService, useValue: teamServiceMock },
        { provide: UserService, useValue: userServiceMock },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(TeamRequestAdminFilterComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();

    expect(component).toBeTruthy();
  });

  it('should load teams and users on init', () => {
    fixture.detectChanges();

    expect(teamServiceMock.getTeams).toHaveBeenCalledWith({ Limit: 1000 });
    expect(userServiceMock.getUser).toHaveBeenCalledWith({ Limit: 1000 });
    expect(component.teams).toEqual([{ id: 1, name: 'Team A' }]);
    expect(component.users).toEqual([{ id: 2, name: 'Alice' }]);
  });

  it('should include admin filters in filter options', () => {
    component.filterTeamId = 1;
    component.filterUserId = 2;
    component.filterStatus = TransactionStatus.Approved;

    const options = component.getFilterOptions()!;

    expect(options.TeamId).toBe(1);
    expect(options.UserId).toBe(2);
    expect(options.Status).toBe(TransactionStatus.Approved);
  });

  it('should emit limitChange when onLimitChange is called', () => {
    fixture.detectChanges();
    let emittedLimit: number | undefined;
    component.limitChange.subscribe((limit) => (emittedLimit = limit));
    component.limit = 50;

    component.onLimitChange();

    expect(emittedLimit).toBe(50);
  });
});
