import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { vi } from 'vitest';

import { CostCentreDto, TeamDto } from '../../../types/exporter';

import { TeamListComponent } from './team-list-component';

describe('TeamListComponent', () => {
  let component: TeamListComponent;
  let fixture: ComponentFixture<TeamListComponent>;
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
      isActive: true,
      members: [mockMember],
      budgets: [
        {
          id: 1,
          name: 'Expired budget',
          description: null,
          costCentreId: 10,
          teamId: 0,
          seasonId: 1,
          targetAmount: 3000,
          periodStart: buildBudgetDate(-120, 'start'),
          periodEnd: buildBudgetDate(-60, 'end'),
          type: 0,
          transactionIds: [],
          paidAmount: 1000,
          approvedAmount: 500,
        },
        {
          id: 2,
          name: 'Vehicle budget',
          description: null,
          costCentreId: 10,
          teamId: 1,
          seasonId: 1,
          targetAmount: 5000,
          periodStart: buildBudgetDate(-30, 'start'),
          periodEnd: buildBudgetDate(30, 'end'),
          type: 0,
          transactionIds: [],
          paidAmount: 1000,
          approvedAmount: 1500,
        },
        {
          id: 4,
          name: 'Operations budget',
          description: null,
          costCentreId: 12,
          teamId: 1,
          seasonId: 1,
          targetAmount: 6000,
          periodStart: buildBudgetDate(-20, 'start'),
          periodEnd: buildBudgetDate(20, 'end'),
          type: 0,
          transactionIds: [],
          paidAmount: 2000,
          approvedAmount: 1500,
        },
        {
          id: 5,
          name: 'Software budget',
          description: null,
          costCentreId: 13,
          teamId: 1,
          seasonId: 1,
          targetAmount: 7000,
          periodStart: buildBudgetDate(-10, 'start'),
          periodEnd: buildBudgetDate(10, 'end'),
          type: 0,
          transactionIds: [],
          paidAmount: 3000,
          approvedAmount: 1500,
        },
        {
          id: 6,
          name: 'Travel budget',
          description: null,
          costCentreId: 14,
          teamId: 1,
          seasonId: 1,
          targetAmount: 8000,
          periodStart: buildBudgetDate(-5, 'start'),
          periodEnd: buildBudgetDate(5, 'end'),
          type: 0,
          transactionIds: [],
          paidAmount: 3000,
          approvedAmount: 2500,
        },
      ],
    },
    {
      id: 2,
      name: 'Operations',
      description: null,
      displayColor: null,
      isActive: false,
      members: null,
      budgets: undefined,
    },
  ];

  const mockCostCentres: CostCentreDto[] = [
    {
      id: 10,
      name: 'Vehicle',
      description: null,
      displayColor: null,
      budgets: [],
      isActive: true,
    },
    {
      id: 12,
      name: 'Operations',
      description: null,
      displayColor: null,
      budgets: [],
      isActive: true,
    },
    {
      id: 13,
      name: 'Software',
      description: null,
      displayColor: null,
      budgets: [],
      isActive: true,
    },
    {
      id: 14,
      name: 'Travel',
      description: null,
      displayColor: null,
      budgets: [],
      isActive: true,
    },
  ];

  const nonMatchingBudgetTeam: TeamDto = {
    id: 3,
    name: 'Finance',
    description: 'No active budget',
    displayColor: '#0f766e',
    isActive: true,
    members: [],
    budgets: [
      {
        id: 3,
        name: 'Future budget',
        description: null,
        costCentreId: 11,
        teamId: 1,
        seasonId: 1,
        targetAmount: 7000,
        periodStart: buildBudgetDate(60, 'start'),
        periodEnd: buildBudgetDate(120, 'end'),
        type: 0,
        transactionIds: [],
        paidAmount: 0,
        approvedAmount: 0,
      },
    ],
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TeamListComponent],
      providers: [provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(TeamListComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create the component', () => {
    expect(component).toBeTruthy();
  });

  it('should emit openEditTeam when onOpenEditTeam is called', () => {
    const spy = vi.spyOn(component.openEditTeam, 'emit');

    component.onOpenEditTeam(mockTeams[0]);

    expect(spy).toHaveBeenCalledOnce();
    expect(spy).toHaveBeenCalledWith(mockTeams[0]);
  });

  it('should return a fallback description when a team has no description', () => {
    expect(component.getDescription(mockTeams[0])).toBe('Builds product features');
    expect(component.getDescription(mockTeams[1])).toBe('No description');
  });

  it('should return a fallback display color when none is configured', () => {
    expect(component.getDisplayColor(mockTeams[0])).toBe('#2563eb');
    expect(component.getDisplayColor(mockTeams[1])).toBe('#f47f1f');
  });

  it('should count members and return all active budgets for the current date', () => {
    expect(component.getMembersCount(mockTeams[0])).toBe(1);
    expect(component.getMembersCount(mockTeams[1])).toBe(0);
    expect(component.getCurrentBudgets(mockTeams[0]).map((budget) => budget.targetAmount)).toEqual([
      5000, 6000, 7000, 8000,
    ]);
    expect(component.getCurrentBudgets(mockTeams[1])).toEqual([]);
    expect(component.getCurrentBudgets(nonMatchingBudgetTeam)).toEqual([]);
  });

  it('should limit visible active budgets to three until expanded', () => {
    expect(
      component.getVisibleCurrentBudgets(mockTeams[0]).map((budget) => budget.targetAmount),
    ).toEqual([5000, 6000, 7000]);
    expect(component.getHiddenCurrentBudgetCount(mockTeams[0])).toBe(1);
    expect(component.hasHiddenCurrentBudgets(mockTeams[0])).toBe(true);

    component.toggleBudgetList(mockTeams[0]);

    expect(
      component.getVisibleCurrentBudgets(mockTeams[0]).map((budget) => budget.targetAmount),
    ).toEqual([5000, 6000, 7000, 8000]);
  });

  it('should include the corresponding cost centre name in the budget display value', () => {
    component.costCentres = mockCostCentres;

    expect(component.getBudgetDisplayValue(mockTeams[0].budgets![1])).toContain(
      'Vehicle: 5.000,00',
    );
    expect(component.getBudgetDisplayValue(nonMatchingBudgetTeam.budgets![0])).toContain(
      'Cost centre #11: 7.000',
    );
  });

  it('should accept @Input teams and render one row per team', () => {
    // Use Angular's input update path so the @empty control-flow block sees the change cleanly.
    fixture.componentRef.setInput('teams', mockTeams);
    fixture.detectChanges();

    const rows = fixture.nativeElement.querySelectorAll('tbody tr');

    expect(component.teams).toEqual(mockTeams);
    expect(rows.length).toBe(2);
  });

  it('should always render the member count and status columns', () => {
    fixture.componentRef.setInput('teams', mockTeams);
    fixture.detectChanges();

    const headers = Array.from(
      fixture.nativeElement.querySelectorAll('thead th') as NodeListOf<HTMLTableCellElement>,
      (header) => header.textContent?.trim(),
    );

    expect(headers).toContain('Member Count');
    expect(headers).toContain('Status');
  });

  it('should render status badges for active and inactive teams', () => {
    fixture.componentRef.setInput('teams', mockTeams);
    fixture.detectChanges();

    const badges = Array.from(
      fixture.nativeElement.querySelectorAll('.status-btn') as NodeListOf<HTMLSpanElement>,
      (badge) => badge.textContent?.trim(),
    );

    expect(badges).toEqual(['Active', 'Inactive']);
  });

  it('should render the member count in each row', () => {
    fixture.componentRef.setInput('teams', mockTeams);
    fixture.componentRef.setInput('costCentres', mockCostCentres);
    fixture.detectChanges();

    const firstRowCells = Array.from(
      fixture.nativeElement.querySelectorAll(
        'tbody tr:first-child td',
      ) as NodeListOf<HTMLTableCellElement>,
      (cell) => cell.textContent?.trim(),
    );
    const secondRowCells = Array.from(
      fixture.nativeElement.querySelectorAll(
        'tbody tr:nth-child(2) td',
      ) as NodeListOf<HTMLTableCellElement>,
      (cell) => cell.textContent?.trim(),
    );

    expect(firstRowCells).toContain('1');
    expect(firstRowCells).toContain('Active');
    expect(secondRowCells).toContain('Inactive');
  });

  it('should render the empty-state row when no teams are available', () => {
    component.teams = [];
    fixture.detectChanges();

    // This protects the user-facing fallback that appears when filters return no results.
    expect(fixture.nativeElement.textContent).toContain('No teams found.');
  });

  it('should keep the empty-state colspan aligned with the visible columns', () => {
    fixture.componentRef.setInput('teams', []);
    fixture.detectChanges();

    const emptyStateCell = fixture.nativeElement.querySelector('tbody tr td');

    expect(emptyStateCell.getAttribute('colspan')).toBe('6');
  });
});

function buildBudgetDate(offsetDays: number, boundary: 'start' | 'end'): string {
  const date = new Date();
  date.setDate(date.getDate() + offsetDays);

  return `${toDateKey(date)}T${boundary === 'start' ? '00:00:00' : '23:59:59'}Z`;
}

function toDateKey(date: Date): string {
  const year = date.getFullYear();
  const month = `${date.getMonth() + 1}`.padStart(2, '0');
  const day = `${date.getDate()}`.padStart(2, '0');
  return `${year}-${month}-${day}`;
}
