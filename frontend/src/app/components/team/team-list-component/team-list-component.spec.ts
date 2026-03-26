import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';

import { TeamService } from '../../../services/team-service';
import { TeamDto } from '../../../types/exporter';

import { TeamListComponent } from './team-list-component';

describe('TeamListComponent', () => {
  let component: TeamListComponent;
  let fixture: ComponentFixture<TeamListComponent>;
  let teamServiceMock: { getTeams: ReturnType<typeof vi.fn> };

  const mockTeams: TeamDto[] = [
    { id: 1, name: 'Team Alpha' },
    { id: 2, name: 'Team Beta' },
  ];

  beforeEach(async () => {
    teamServiceMock = { getTeams: vi.fn() };

    await TestBed.configureTestingModule({
      imports: [TeamListComponent],
      providers: [{ provide: TeamService, useValue: teamServiceMock }],
    }).compileComponents();

    fixture = TestBed.createComponent(TeamListComponent);
    component = fixture.componentInstance;
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  describe('initialization', () => {
    it('should create the component', () => {
      teamServiceMock.getTeams.mockReturnValue(of([]));
      fixture.detectChanges();
      expect(component).toBeTruthy();
    });

    it('should start with an empty teams signal', () => {
      teamServiceMock.getTeams.mockReturnValue(of([]));
      expect(component.teams()).toEqual([]);
    });

    it('should call loadTeams on init', () => {
      teamServiceMock.getTeams.mockReturnValue(of(mockTeams));
      fixture.detectChanges();
      expect(teamServiceMock.getTeams).toHaveBeenCalledTimes(1);
    });
  });

  describe('loadTeams()', () => {
    it('should populate the teams signal on success', () => {
      teamServiceMock.getTeams.mockReturnValue(of(mockTeams));
      fixture.detectChanges();
      expect(component.teams()).toEqual(mockTeams);
    });

    it('should log an error and not update teams on failure', () => {
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
      teamServiceMock.getTeams.mockReturnValue(throwError(() => new Error('Network error')));
      fixture.detectChanges();
      expect(component.teams()).toEqual([]);
      expect(consoleSpy).toHaveBeenCalled();
    });

    it('should refresh teams when called again', () => {
      teamServiceMock.getTeams.mockReturnValue(of(mockTeams));
      fixture.detectChanges();

      const updatedTeams: TeamDto[] = [{ id: 3, name: 'Team Gamma' }];
      teamServiceMock.getTeams.mockReturnValue(of(updatedTeams));
      component.loadTeams();
      expect(component.teams()).toEqual(updatedTeams);
    });
  });
});
