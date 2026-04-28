import { ComponentFixture, TestBed } from '@angular/core/testing';
import { vi } from 'vitest';

import { TeamDto } from '../../../types/exporter';

import { TeamListComponent } from './team-list-component';

describe('TeamListComponent', () => {
  let component: TeamListComponent;
  let fixture: ComponentFixture<TeamListComponent>;
  const mockMember: NonNullable<TeamDto['members']>[number] = {
    id: 1,
    name: 'Alice',
    email: 'alice@test.com',
    profilePictureUrl: 'https://example.com/alice.png',
    role: 0,
    team: {} as TeamDto,
    isActive: true,
  };

  const mockTeams: TeamDto[] = [
    {
      id: 1,
      name: 'Platform',
      description: 'Builds product features',
      displayColor: '#2563eb',
      members: [mockMember],
      budgets: [
        {
          id: 1,
          costCentreId: 10,
          targetAmount: 3000,
          periodStart: buildBudgetDate(-120, 'start'),
          periodEnd: buildBudgetDate(-60, 'end'),
        },
        {
          id: 2,
          costCentreId: 10,
          targetAmount: 5000,
          periodStart: buildBudgetDate(-30, 'start'),
          periodEnd: buildBudgetDate(30, 'end'),
        },
      ],
    },
    {
      id: 2,
      name: 'Operations',
      description: null,
      displayColor: null,
      members: null,
      budgets: undefined,
    },
  ];

  const nonMatchingBudgetTeam: TeamDto = {
    id: 3,
    name: 'Finance',
    description: 'No active budget',
    displayColor: '#0f766e',
    members: [],
    budgets: [
      {
        id: 3,
        costCentreId: 11,
        targetAmount: 7000,
        periodStart: buildBudgetDate(60, 'start'),
        periodEnd: buildBudgetDate(120, 'end'),
      },
    ],
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TeamListComponent],
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
    expect(component.getDisplayColor(mockTeams[1])).toBe('transparent');
  });

  it('should count members and return the active target amount for the current date', () => {
    expect(component.getMembersCount(mockTeams[0])).toBe(1);
    expect(component.getMembersCount(mockTeams[1])).toBe(0);
    expect(component.getBudgetTargetAmount(mockTeams[0])).toBe(5000);
    expect(component.getBudgetTargetAmount(mockTeams[1])).toBeNull();
    expect(component.getBudgetTargetAmount(nonMatchingBudgetTeam)).toBeNull();
  });

  it('should accept @Input teams and render one row per team', () => {
    // Use Angular's input update path so the @empty control-flow block sees the change cleanly.
    fixture.componentRef.setInput('teams', mockTeams);
    fixture.detectChanges();

    const rows = fixture.nativeElement.querySelectorAll('tbody tr');

    expect(component.teams).toEqual(mockTeams);
    expect(rows.length).toBe(2);
  });

  it('should always render the member count and team budget columns', () => {
    fixture.componentRef.setInput('teams', mockTeams);
    fixture.detectChanges();

    const headers = Array.from(
      fixture.nativeElement.querySelectorAll('thead th') as NodeListOf<HTMLTableCellElement>,
      (header) => header.textContent?.trim(),
    );

    expect(headers).toContain('Member Count');
    expect(headers).toContain('Team Budget');
  });

  it('should render the member count and current team budget in each row', () => {
    fixture.componentRef.setInput('teams', mockTeams);
    fixture.detectChanges();

    const columns = fixture.nativeElement.querySelectorAll('colgroup col');
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

    expect(columns.length).toBe(5);
    expect(firstRowCells).toContain('1');
    expect(firstRowCells).toContain('5000 €');
    expect(secondRowCells).toContain('No budget set');
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

    expect(emptyStateCell.getAttribute('colspan')).toBe('5');
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
