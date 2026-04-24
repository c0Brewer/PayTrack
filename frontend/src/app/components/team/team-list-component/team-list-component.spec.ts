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
      budget: {
        id: 1,
        costCentreId: 10,
        targetAmount: 5000,
        periodStart: '2026-01-01T00:00:00Z',
        periodEnd: '2026-12-31T00:00:00Z',
      },
    },
    {
      id: 2,
      name: 'Operations',
      description: null,
      displayColor: null,
      members: null,
      budget: undefined,
    },
  ];

  const legacyBudgetTeam: TeamDto = {
    id: 3,
    name: 'Finance',
    description: 'Legacy payload shape',
    displayColor: '#0f766e',
    members: [],
    budget: [
      {
        id: 2,
        costCentreId: 11,
        targetAmount: 7000,
        periodStart: '2026-01-01T00:00:00Z',
        periodEnd: '2026-12-31T00:00:00Z',
      },
    ] as unknown as TeamDto['budget'],
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

  it('should count members and read the target amount defensively when relations are missing', () => {
    expect(component.getMembersCount(mockTeams[0])).toBe(1);
    expect(component.getMembersCount(mockTeams[1])).toBe(0);
    expect(component.getBudgetTargetAmount(mockTeams[0])).toBe(5000);
    expect(component.getBudgetTargetAmount(mockTeams[1])).toBeNull();
    expect(component.getBudgetTargetAmount(legacyBudgetTeam)).toBe(7000);
  });

  it('should accept @Input teams and render one row per team', () => {
    // Use Angular's input update path so the @empty control-flow block sees the change cleanly.
    fixture.componentRef.setInput('teams', mockTeams);
    fixture.detectChanges();

    const rows = fixture.nativeElement.querySelectorAll('tbody tr');

    expect(component.teams).toEqual(mockTeams);
    expect(rows.length).toBe(2);
  });

  it('should always render the members and target amount columns', () => {
    fixture.componentRef.setInput('teams', mockTeams);
    fixture.detectChanges();

    const headers = Array.from(
      fixture.nativeElement.querySelectorAll('thead th') as NodeListOf<HTMLTableCellElement>,
      (header) => header.textContent?.trim(),
    );

    expect(headers).toContain('Members');
    expect(headers).toContain('Target Amount');
  });

  it('should render members and the current budget target amount in each row', () => {
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
    expect(secondRowCells).toContain('No target amount');
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
