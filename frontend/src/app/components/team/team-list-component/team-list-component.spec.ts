import { ComponentFixture, TestBed } from '@angular/core/testing';
import { vi } from 'vitest';

import { TeamDto } from '../../../types/exporter';

import { TeamListComponent } from './team-list-component';

describe('TeamListComponent', () => {
  let component: TeamListComponent;
  let fixture: ComponentFixture<TeamListComponent>;

  const mockTeams: TeamDto[] = [
    {
      id: 1,
      name: 'Platform',
      description: 'Builds product features',
      displayColor: '#2563eb',
      members: [{ id: 1, name: 'Alice', email: 'alice@test.com', role: 0, isActive: true }],
      budgets: [
        {
          id: 1,
          costCentreId: 10,
          targetAmount: 5000,
          periodStart: '2026-01-01T00:00:00Z',
          periodEnd: '2026-12-31T00:00:00Z',
        },
      ],
    },
    {
      id: 2,
      name: 'Operations',
      description: null,
      displayColor: null,
      members: null,
      budgets: null,
    },
  ];

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

  it('should count members and budgets defensively when relations are missing', () => {
    expect(component.getMembersCount(mockTeams[0])).toBe(1);
    expect(component.getMembersCount(mockTeams[1])).toBe(0);
    expect(component.getBudgetsCount(mockTeams[0])).toBe(1);
    expect(component.getBudgetsCount(mockTeams[1])).toBe(0);
  });

  it('should accept @Input teams and render one row per team', () => {
    // Use Angular's input update path so the @empty control-flow block sees the change cleanly.
    fixture.componentRef.setInput('teams', mockTeams);
    fixture.detectChanges();

    const rows = fixture.nativeElement.querySelectorAll('tbody tr');

    expect(component.teams).toEqual(mockTeams);
    expect(rows.length).toBe(2);
  });

  it('should render the empty-state row when no teams are available', () => {
    component.teams = [];
    fixture.detectChanges();

    // This protects the user-facing fallback that appears when filters return no results.
    expect(fixture.nativeElement.textContent).toContain('No teams found.');
  });
});
