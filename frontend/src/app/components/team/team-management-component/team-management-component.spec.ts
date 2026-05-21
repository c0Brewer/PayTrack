import { ChangeDetectorRef } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';

import { CostCentreService } from '../../../services/cost-centre/cost-centre-service';
import { NotificationService } from '../../../services/notification/notification-service';
import { SeasonService } from '../../../services/season/season-service';
import { TeamService } from '../../../services/team/team-service';
import { CostCentreDtoPaginatedResponse, SeasonDto, TeamDto } from '../../../types/exporter';
import { StatBoxComponent } from '../../general/boxes/stat-box-component/stat-box-component';
import { TeamEditModalComponent } from '../team-edit-modal-component/team-edit-modal-component';
import { TeamFilterComponent } from '../team-filter-component/team-filter-component';
import { TeamListComponent } from '../team-list-component/team-list-component';

import { TeamManagementComponent } from './team-management-component';

describe('TeamManagementComponent', () => {
  let component: TeamManagementComponent;
  let fixture: ComponentFixture<TeamManagementComponent>;
  let teamServiceMock: {
    getTeams: ReturnType<typeof vi.fn>;
    createTeam: ReturnType<typeof vi.fn>;
    updateTeam: ReturnType<typeof vi.fn>;
    getDeleteImpact: ReturnType<typeof vi.fn>;
    deleteTeam: ReturnType<typeof vi.fn>;
  };
  let costCentreServiceMock: {
    getCostCentres: ReturnType<typeof vi.fn>;
  };
  let seasonServiceMock: {
    getSeasons: ReturnType<typeof vi.fn>;
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
  const mockCostCentres: CostCentreDtoPaginatedResponse = {
    items: [
      { id: 1, name: 'Aerodynamics', description: 'Aero', displayColor: '#ff5733', budgets: [] },
      { id: 2, name: 'Operations', description: 'Ops', displayColor: '#0f766e', budgets: [] },
    ],
    totalCount: 2,
    limit: 1000,
    offset: 0,
    hasNext: false,
    hasPrevious: false,
  };
  const mockSeasons: SeasonDto[] = [
    { id: 1, name: '2025', budgets: [] },
    { id: 2, name: '2026', budgets: [] },
  ];

  beforeEach(async () => {
    teamServiceMock = {
      getTeams: vi
        .fn()
        .mockReturnValue(
          of({ items: mockTeams, totalCount: 2, hasNext: false, hasPrevious: false }),
        ),
      createTeam: vi.fn().mockReturnValue(of({})),
      updateTeam: vi.fn().mockReturnValue(of({})),
      getDeleteImpact: vi.fn().mockReturnValue(
        of({
          teamId: 1,
          teamName: 'Platform',
          canDelete: true,
          affectedUserCount: 0,
          blockingBudgetCount: 0,
          blockingTransactionCount: 0,
          invoiceCount: 0,
          warningMessage: '',
        }),
      ),
      deleteTeam: vi.fn().mockReturnValue(of(null)),
    };
    costCentreServiceMock = {
      getCostCentres: vi.fn().mockReturnValue(of(mockCostCentres)),
    };
    seasonServiceMock = {
      getSeasons: vi.fn().mockReturnValue(of(mockSeasons)),
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
        { provide: CostCentreService, useValue: costCentreServiceMock },
        { provide: SeasonService, useValue: seasonServiceMock },
        { provide: TeamService, useValue: teamServiceMock },
        { provide: NotificationService, useValue: notificationServiceMock },
        { provide: ChangeDetectorRef, useValue: cdrMock },
        provideRouter([]),
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

    expect(costCentreServiceMock.getCostCentres).toHaveBeenCalledWith({
      Limit: 1000,
      Offset: 0,
    });
    expect(seasonServiceMock.getSeasons).toHaveBeenCalled();
    expect(component.seasons).toEqual(mockSeasons);
    expect(teamServiceMock.getTeams).toHaveBeenCalledWith({
      Name: undefined,
      Description: undefined,
      IsActive: undefined,
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
      IsActive: true,
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
      IsActive: true,
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

  it('loadSeasons should surface service errors through the notification service', () => {
    seasonServiceMock.getSeasons.mockReturnValueOnce(throwError(() => new Error('Season failed')));

    component.loadSeasons();

    expect(notificationServiceMock.showError).toHaveBeenCalledWith(
      'Could not load seasons: Season failed',
    );
  });

  it('updateFilterOptions should merge filters, reset the page, and reload the list', () => {
    const loadTeamsSpy = vi.spyOn(component, 'loadTeams');
    component.page = 3;

    component.updateFilterOptions({
      Name: 'Operations',
      Description: 'running',
      IsActive: true,
    });

    expect(component.filterOptions).toEqual(
      expect.objectContaining({
        Name: 'Operations',
        Description: 'running',
        IsActive: true,
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

  it('openCreate should initialize a new team draft like the cost centre flow', () => {
    component.openCreate();

    expect(component.editingTeam).toEqual({
      id: -1,
      name: '',
      description: '',
      displayColor: '#2563eb',
      isActive: true,
      members: [],
      budgets: [],
    });
  });

  it('closeEdit should reset editingTeam', () => {
    component.editingTeam = { ...mockTeams[0] };

    component.closeEdit();

    expect(component.editingTeam).toBeNull();
  });

  it('saveTeam should call updateTeam, reload the list, show success, and close the modal', () => {
    const loadTeamsSpy = vi.spyOn(component, 'loadTeams');
    component.editingTeam = { ...mockTeams[0] };

    component.saveTeam({ team: mockTeams[0], budgetsToUpsert: [], budgetIdsToDelete: [] });

    expect(teamServiceMock.updateTeam).toHaveBeenCalledWith(mockTeams[0].id, {
      name: mockTeams[0].name,
      description: mockTeams[0].description,
      displayColor: mockTeams[0].displayColor,
      budgetsToUpsert: undefined,
      budgetIdsToDelete: undefined,
    });
    expect(notificationServiceMock.showSuccess).toHaveBeenCalledWith(
      'Successfully updated team ' + mockTeams[0].name,
    );
    expect(loadTeamsSpy).toHaveBeenCalledOnce();
    expect(component.editingTeam).toBeNull();
  });

  it('saveTeam should call createTeam for new teams, reload the list, show success, and close the modal', () => {
    const loadTeamsSpy = vi.spyOn(component, 'loadTeams');
    component.editingTeam = {
      id: -1,
      name: 'New Team',
      description: 'Freshly created',
      displayColor: '#2563eb',
      members: [],
      budgets: [],
    };

    component.saveTeam({ team: component.editingTeam, budgetsToUpsert: [], budgetIdsToDelete: [] });

    expect(teamServiceMock.createTeam).toHaveBeenCalledWith({
      name: 'New Team',
      description: 'Freshly created',
      displayColor: '#2563eb',
      budgets: undefined,
    });
    expect(notificationServiceMock.showSuccess).toHaveBeenCalledWith(
      'Successfully created team New Team',
    );
    expect(loadTeamsSpy).toHaveBeenCalledOnce();
    expect(component.editingTeam).toBeNull();
  });

  it('saveTeam should surface update errors through the notification service', () => {
    teamServiceMock.updateTeam.mockReturnValueOnce(throwError(() => new Error('Update failed')));

    component.saveTeam({ team: mockTeams[0], budgetsToUpsert: [], budgetIdsToDelete: [] });

    expect(notificationServiceMock.showError).toHaveBeenCalledWith(
      'Could not update Team: Error: Update failed',
    );
  });

  it('saveTeam should surface create errors through the notification service', () => {
    teamServiceMock.createTeam.mockReturnValueOnce(throwError(() => new Error('Create failed')));

    component.saveTeam({
      team: {
        id: -1,
        name: 'New Team',
        description: 'Freshly created',
        displayColor: '#2563eb',
        members: [],
        budgets: [],
      },
      budgetsToUpsert: [],
      budgetIdsToDelete: [],
    });

    expect(notificationServiceMock.showError).toHaveBeenCalledWith(
      'Could not create Team: Error: Create failed',
    );
  });

  it('saveTeam should include budgets in create requests when provided', () => {
    component.saveTeam({
      team: {
        id: -1,
        name: 'New Team',
        description: 'Freshly created',
        displayColor: '#2563eb',
        members: [],
        budgets: [],
      },
      budgetsToUpsert: [
        {
          id: null,
          name: 'New budget',
          description: null,
          costCentreId: 2,
          seasonId: 1,
          targetAmount: 500,
          periodStart: '2026-01-01',
          periodEnd: '2026-06-30',
        },
      ],
      budgetIdsToDelete: [],
    });

    expect(teamServiceMock.createTeam).toHaveBeenCalledWith(
      expect.objectContaining({
        budgets: [
          {
            name: 'New budget',
            description: null,
            costCentreId: 2,
            seasonId: 1,
            targetAmount: 500,
            periodStart: '2026-01-01T00:00:00.000Z',
            periodEnd: '2026-06-30T00:00:00.000Z',
          },
        ],
      }),
    );
  });

  it('saveTeam should include budget upserts and deletions in update requests', () => {
    component.saveTeam({
      team: mockTeams[0],
      budgetsToUpsert: [
        {
          id: 10,
          name: 'Updated budget',
          description: null,
          costCentreId: 2,
          seasonId: 1,
          targetAmount: 500,
          periodStart: '2026-01-01',
          periodEnd: '2026-06-30',
        },
      ],
      budgetIdsToDelete: [15],
    });

    expect(teamServiceMock.updateTeam).toHaveBeenCalledWith(
      mockTeams[0].id,
      expect.objectContaining({
        budgetsToUpsert: [
          {
            id: 10,
            name: 'Updated budget',
            description: null,
            costCentreId: 2,
            seasonId: 1,
            targetAmount: 500,
            periodStart: '2026-01-01T00:00:00.000Z',
            periodEnd: '2026-06-30T00:00:00.000Z',
          },
        ],
        budgetIdsToDelete: [15],
      }),
    );
  });

  it('openDeleteTeam should load delete impact, close edit, and open the impact modal state', () => {
    component.editingTeam = { ...mockTeams[0] };

    component.openDeleteTeam(mockTeams[0]);

    expect(teamServiceMock.getDeleteImpact).toHaveBeenCalledWith(mockTeams[0].id);
    expect(component.editingTeam).toBeNull();
    expect(component.deletingTeam).toEqual(mockTeams[0]);
    expect(component.deleteImpact).toEqual(
      expect.objectContaining({
        teamName: 'Platform',
        canDelete: true,
      }),
    );
  });

  it('openDeleteTeam should surface delete impact errors', () => {
    teamServiceMock.getDeleteImpact.mockReturnValueOnce(
      throwError(() => new Error('Impact failed')),
    );

    component.openDeleteTeam(mockTeams[0]);

    expect(notificationServiceMock.showError).toHaveBeenCalledWith(
      'Could not load delete impact: Impact failed',
    );
  });

  it('confirmDelete should delete teams without impact', () => {
    const loadTeamsSpy = vi.spyOn(component, 'loadTeams');
    component.deletingTeam = mockTeams[0];
    component.deleteImpact = {
      teamId: 1,
      teamName: 'Platform',
      canDelete: true,
      affectedUserCount: 0,
      blockingBudgetCount: 0,
      blockingTransactionCount: 0,
      invoiceCount: 0,
      warningMessage: '',
    };

    component.confirmDelete();

    expect(teamServiceMock.deleteTeam).toHaveBeenCalledWith(mockTeams[0].id);
    expect(notificationServiceMock.showSuccess).toHaveBeenCalledWith(
      'Team "Platform" deleted successfully',
    );
    expect(component.deletingTeam).toBeNull();
    expect(component.deleteImpact).toBeNull();
    expect(loadTeamsSpy).toHaveBeenCalledOnce();
  });

  it('confirmDelete should report deactivation when the delete endpoint returns a team', () => {
    teamServiceMock.deleteTeam.mockReturnValueOnce(of({ ...mockTeams[0], isActive: false }));
    component.deletingTeam = mockTeams[0];

    component.confirmDelete();

    expect(notificationServiceMock.showSuccess).toHaveBeenCalledWith('Team "Platform" deactivated');
  });

  it('confirmDelete should surface delete errors', () => {
    teamServiceMock.deleteTeam.mockReturnValueOnce(throwError(() => new Error('Delete failed')));
    component.deletingTeam = mockTeams[0];

    component.confirmDelete();

    expect(notificationServiceMock.showError).toHaveBeenCalledWith(
      'Could not delete team: Delete failed',
    );
  });

  it('should pass loaded teams into the list child component', () => {
    fixture.detectChanges();

    const listComponent = fixture.debugElement.query(
      By.directive(TeamListComponent),
    ).componentInstance;

    expect(listComponent.teams).toEqual(mockTeams);
  });

  it('should pass the total team count into the total teams stat box', () => {
    fixture.detectChanges();

    const statBox = fixture.debugElement.query(By.directive(StatBoxComponent)).componentInstance;

    expect(statBox.header()).toBe('Total Teams');
    expect(statBox.content()).toBe(2);
  });

  it('should keep members and budgets included when filters change', () => {
    fixture.detectChanges();

    component.updateFilterOptions({ Name: 'Operations' });

    expect(teamServiceMock.getTeams).toHaveBeenLastCalledWith(
      expect.objectContaining({
        Name: 'Operations',
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
    expect(teamServiceMock.getTeams).toHaveBeenCalledTimes(3);
  });

  it('should keep the total teams stat unchanged when filters change', () => {
    fixture.detectChanges();
    teamServiceMock.getTeams.mockReturnValueOnce(
      of({ items: [mockTeams[0]], totalCount: 1, hasNext: false, hasPrevious: false }),
    );

    component.updateFilterOptions({ Name: 'Platform' });
    fixture.detectChanges();

    const statBox = fixture.debugElement.query(By.directive(StatBoxComponent)).componentInstance;

    expect(component.totalCount).toBe(1);
    expect(component.totalTeamCount).toBe(2);
    expect(statBox.content()).toBe(2);
  });

  it('should route edit modal delete requests through the delete impact flow', () => {
    const openDeleteSpy = vi.spyOn(component, 'openDeleteTeam');
    const editComponent = TestBed.createComponent(TeamEditModalComponent).componentInstance;
    editComponent.deleteEvent.subscribe((team) => component.openDeleteTeam(team));

    editComponent.deleteEvent.emit(mockTeams[0]);

    expect(openDeleteSpy).toHaveBeenCalledWith(mockTeams[0]);
    expect(teamServiceMock.getDeleteImpact).toHaveBeenCalledWith(mockTeams[0].id);
  });
});
