import { ChangeDetectorRef } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';

import { NotificationService } from '../../../services/notification/notification-service';
import { TeamService } from '../../../services/team/team-service';
import { TeamDto } from '../../../types/exporter';
import { TeamFilterComponent } from '../team-filter-component/team-filter-component';
import { TeamListComponent } from '../team-list-component/team-list-component';

import { TeamManagementComponent } from './team-management-component';

describe('TeamManagementComponent', () => {
  let component: TeamManagementComponent;
  let fixture: ComponentFixture<TeamManagementComponent>;
  let teamServiceMock: {
    getTeams: ReturnType<typeof vi.fn>;
    updateTeam: ReturnType<typeof vi.fn>;
  };
  let notificationServiceMock: {
    showError: ReturnType<typeof vi.fn>;
    showSuccess: ReturnType<typeof vi.fn>;
  };
  let cdrMock: {
    markForCheck: ReturnType<typeof vi.fn>;
  };
  const mockMember: NonNullable<TeamDto['members']>[number] = {
    id: 1,
    name: 'Alice',
    email: 'alice@test.com',
    profilePictureUrl: 'https://example.com/alice.png',
    bankAccounts: [],
    role: 0,
    team: {} as TeamDto,
    isActive: true,
    bankInformationSkipped: true,
    hasBankInformation: true,
  };

  const mockTeams: TeamDto[] = [
    {
      id: 1,
      name: 'Platform',
      description: 'Builds product features',
      displayColor: '#2563eb',
      members: [mockMember],
      budgets: undefined,
    },
    {
      id: 2,
      name: 'Operations',
      description: 'Keeps things running',
      displayColor: null,
      members: [],
      budgets: undefined,
    },
  ];

  beforeEach(async () => {
    teamServiceMock = {
      getTeams: vi
        .fn()
        .mockReturnValue(
          of({ items: mockTeams, totalCount: 2, hasNext: false, hasPrevious: false }),
        ),
      updateTeam: vi.fn().mockReturnValue(of({})),
    };

    notificationServiceMock = {
      showError: vi.fn(),
      showSuccess: vi.fn(),
    };

    cdrMock = {
      markForCheck: vi.fn(),
    };

    await TestBed.configureTestingModule({
      imports: [TeamManagementComponent],
      providers: [
        { provide: TeamService, useValue: teamServiceMock },
        { provide: NotificationService, useValue: notificationServiceMock },
        { provide: ChangeDetectorRef, useValue: cdrMock },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(TeamManagementComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('ngOnInit should load teams and initialize pagination metadata', () => {
    component.ngOnInit();

    expect(teamServiceMock.getTeams).toHaveBeenCalledWith({
      Name: undefined,
      Description: undefined,
      MinBudget: undefined,
      MaxBudget: undefined,
      IncludeMembers: true,
      IncludeBudgets: true,
      Limit: 10,
      Offset: 0,
    });
    expect(component.teams).toEqual(mockTeams);
    expect(component.totalCount).toBe(2);
    expect(component.hasNext).toBe(false);
    expect(component.hasPrev).toBe(false);
  });

  it('loadTeams should forward the current filter and pagination state to the service', () => {
    component.filterOptions = {
      Name: 'Platform',
      Description: 'Builds',
      MinBudget: 100,
      MaxBudget: 500,
      IncludeMembers: false,
      IncludeBudgets: false,
      Limit: undefined,
      Offset: undefined,
    };
    component.limit = 25;
    component.page = 2;

    component.loadTeams();

    expect(teamServiceMock.getTeams).toHaveBeenCalledWith({
      Name: 'Platform',
      Description: 'Builds',
      MinBudget: 100,
      MaxBudget: 500,
      IncludeMembers: true,
      IncludeBudgets: true,
      Limit: 25,
      Offset: 50,
    });
  });

  it('loadTeams should surface an empty payload as a user-facing error', () => {
    teamServiceMock.getTeams.mockReturnValueOnce(of({ items: null }));

    component.loadTeams();

    expect(notificationServiceMock.showError).toHaveBeenCalledWith('Error while loading Items');
  });

  it('loadTeams should forward service errors to the notification service', () => {
    teamServiceMock.getTeams.mockReturnValueOnce(throwError(() => new Error('API error')));

    component.loadTeams();

    expect(notificationServiceMock.showError).toHaveBeenCalledWith(expect.any(Error));
  });

  it('updateFilterOptions should merge filters, reset the page, and reload the list', () => {
    const loadTeamsSpy = vi.spyOn(component, 'loadTeams');
    component.page = 3;

    component.updateFilterOptions({
      Name: 'Operations',
      Description: 'running',
      MinBudget: 50,
      MaxBudget: 900,
    });

    expect(component.filterOptions).toEqual(
      expect.objectContaining({
        Name: 'Operations',
        Description: 'running',
        MinBudget: 50,
        MaxBudget: 900,
        IncludeMembers: true,
        IncludeBudgets: true,
      }),
    );
    expect(component.page).toBe(0);
    expect(loadTeamsSpy).toHaveBeenCalledOnce();
  });

  it('onLimitChange should update the limit, reset the page, and reload', () => {
    const loadTeamsSpy = vi.spyOn(component, 'loadTeams');
    component.page = 4;

    component.onLimitChange(50);

    expect(component.limit).toBe(50);
    expect(component.page).toBe(0);
    expect(loadTeamsSpy).toHaveBeenCalledOnce();
  });

  it('nextPage should increment the page and reload the list', () => {
    const loadTeamsSpy = vi.spyOn(component, 'loadTeams');

    component.nextPage();

    expect(component.page).toBe(1);
    expect(loadTeamsSpy).toHaveBeenCalledOnce();
  });

  it('previousPage should only decrement when the current page is above zero', () => {
    const loadTeamsSpy = vi.spyOn(component, 'loadTeams');
    component.page = 1;

    component.previousPage();
    component.previousPage();

    expect(component.page).toBe(0);
    expect(loadTeamsSpy).toHaveBeenCalledOnce();
  });

  it('getTotalPages should always return at least one page', () => {
    component.totalCount = 23;
    component.limit = 10;
    expect(component.getTotalPages()).toBe(3);

    component.totalCount = 0;
    expect(component.getTotalPages()).toBe(1);
  });

  it('openEditTeam should clone the selected team into parent-managed state', () => {
    component.openEditTeam(mockTeams[0]);

    expect(component.editingTeam).toEqual(mockTeams[0]);
    expect(component.editingTeam).not.toBe(mockTeams[0]);
  });

  it('closeEdit should reset editingTeam', () => {
    component.editingTeam = { ...mockTeams[0] };

    component.closeEdit();

    expect(component.editingTeam).toBeNull();
  });

  it('saveTeam should call updateTeam, reload the list, show success, and close the modal', () => {
    const loadTeamsSpy = vi.spyOn(component, 'loadTeams');
    component.editingTeam = { ...mockTeams[0] };

    component.saveTeam(mockTeams[0]);

    expect(teamServiceMock.updateTeam).toHaveBeenCalledWith(mockTeams[0].id, {
      name: mockTeams[0].name,
      description: mockTeams[0].description,
      displayColor: mockTeams[0].displayColor,
    });
    expect(notificationServiceMock.showSuccess).toHaveBeenCalledWith(
      'Successfully updated team ' + mockTeams[0].name,
    );
    expect(loadTeamsSpy).toHaveBeenCalledOnce();
    expect(component.editingTeam).toBeNull();
  });

  it('saveTeam should surface update errors through the notification service', () => {
    teamServiceMock.updateTeam.mockReturnValueOnce(throwError(() => new Error('Update failed')));

    component.saveTeam(mockTeams[0]);

    expect(notificationServiceMock.showError).toHaveBeenCalledWith(
      'Could not update Team: Error: Update failed',
    );
  });

  it('should pass loaded teams into the list child component', () => {
    fixture.detectChanges();

    const listComponent = fixture.debugElement.query(
      By.directive(TeamListComponent),
    ).componentInstance;

    expect(listComponent.teams).toEqual(mockTeams);
  });

  it('should keep members and budgets included when filters change', () => {
    fixture.detectChanges();

    component.updateFilterOptions({ Name: 'Operations', MaxBudget: 900 });

    expect(teamServiceMock.getTeams).toHaveBeenLastCalledWith(
      expect.objectContaining({
        Name: 'Operations',
        MaxBudget: 900,
        IncludeMembers: true,
        IncludeBudgets: true,
      }),
    );
  });

  it('should react to the list child edit event in the parent component', () => {
    fixture.detectChanges();

    // Emit from the child component to verify the data-down/events-up contract.
    const listComponent = fixture.debugElement.query(
      By.directive(TeamListComponent),
    ).componentInstance;
    listComponent.openEditTeam.emit(mockTeams[0]);

    expect(component.editingTeam).toEqual(mockTeams[0]);
  });

  it('should react to the filter child event and trigger a reload with new filters', () => {
    fixture.detectChanges();

    // Emit from the child filter to prove the parent listens through the template binding.
    const filterComponent = fixture.debugElement.query(
      By.directive(TeamFilterComponent),
    ).componentInstance;
    filterComponent.updateFilter.emit({ Name: 'Platform' });

    expect(component.filterOptions).toEqual(
      expect.objectContaining({
        Name: 'Platform',
        IncludeMembers: true,
        IncludeBudgets: true,
      }),
    );
    expect(teamServiceMock.getTeams).toHaveBeenCalledTimes(2);
  });
});
