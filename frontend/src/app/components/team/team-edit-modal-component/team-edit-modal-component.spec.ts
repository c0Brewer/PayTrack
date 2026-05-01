import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TeamDto } from '../../../types/exporter';

import { TeamEditModalComponent } from './team-edit-modal-component';

describe('TeamEditModalComponent', () => {
  let component: TeamEditModalComponent;
  let fixture: ComponentFixture<TeamEditModalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TeamEditModalComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(TeamEditModalComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('ngOnChanges should deep clone team to originalTeam', () => {
    const team: TeamDto = {
      id: 1,
      name: 'Platform',
      description: 'Builds product features',
      displayColor: '#2563eb',
      members: [],
      budgets: [],
    };

    component.team = team;
    component.ngOnChanges();

    expect(component.originalTeam).not.toBeNull();
    expect(component.originalTeam).toEqual(team);
    expect(component.originalTeam).not.toBe(team);
  });

  it('hasTeamBeenChanged should detect changes', () => {
    component.team.name = 'Platform';
    component.originalTeam = { ...component.team, name: 'Operations' };

    expect(component.hasTeamBeenChanged()).toBe(true);

    component.originalTeam.name = 'Platform';
    expect(component.hasTeamBeenChanged()).toBe(false);
  });

  it('selectedColor should fall back to the default color for invalid values', () => {
    component.team.displayColor = 'blue';

    expect(component.selectedColor).toBe(component.defaultColor);
  });

  it('setDisplayColor should only accept valid hex colors', () => {
    component.team.displayColor = '#2563eb';

    component.setDisplayColor('#dc2626');
    expect(component.team.displayColor).toBe('#dc2626');

    component.setDisplayColor('red');
    expect(component.team.displayColor).toBe('#dc2626');
  });

  it('onFieldBlur should mark a field as touched', () => {
    component.onFieldBlur('name');

    expect(component.touchedFields.name).toBe(true);
  });

  it('hasFieldError should require both touch state and invalid input', () => {
    component.team.name = '';

    expect(component.hasFieldError('name')).toBe(false);

    component.onFieldBlur('name');

    expect(component.hasFieldError('name')).toBe(true);
  });

  it('getFieldError should require a name', () => {
    component.team.name = ' ';

    expect(component.getFieldError('name')).toBe('Name is required.');
  });

  it('getFieldError should reject names shorter than three characters', () => {
    component.team.name = 'ab';

    expect(component.getFieldError('name')).toBe('Name must be at least 3 characters long.');
  });

  it('getFieldError should reject descriptions shorter than three characters', () => {
    component.team.description = 'ab';

    expect(component.getFieldError('description')).toBe(
      'Description must be at least 3 characters long.',
    );
  });

  it('onSave should emit saveEvent if team changed', () => {
    component.team.name = 'Platform';
    component.originalTeam = { ...component.team, name: 'Operations' };
    const emitSpy = vi.spyOn(component.saveEvent, 'emit');

    component.onSave();

    expect(emitSpy).toHaveBeenCalledWith({
      team: component.team,
      budgetsToUpsert: [],
      budgetIdsToDelete: [],
    });
  });

  it('onSave should not emit when validation fails and should mark fields as touched', () => {
    component.team.name = 'ab';
    component.team.description = 'ab';
    const emitSpy = vi.spyOn(component.saveEvent, 'emit');

    component.onSave();

    expect(component.touchedFields.name).toBe(true);
    expect(component.touchedFields.description).toBe(true);
    expect(emitSpy).not.toHaveBeenCalled();
  });

  it('onSave should close the modal if team is unchanged', () => {
    component.team = {
      id: 1,
      name: 'Platform',
      description: 'Builds product features',
      displayColor: '#2563eb',
      members: [],
      budgets: [],
    };
    component.originalTeam = { ...component.team };
    const closeSpy = vi.spyOn(component, 'onClose');

    component.onSave();

    expect(closeSpy).toHaveBeenCalledOnce();
  });

  it('onClose should emit closeEvent', () => {
    const emitSpy = vi.spyOn(component.closeEvent, 'emit');

    component.onClose();

    expect(emitSpy).toHaveBeenCalledOnce();
  });

  it('onSave should include edited and new budgets plus deletions', () => {
    component.team = {
      id: 1,
      name: 'Platform',
      description: 'Builds product features',
      displayColor: '#2563eb',
      members: [],
      budgets: [
        {
          id: 10,
          teamId: 1,
          costCentreId: 2,
          targetAmount: 100,
          periodStart: '2026-01-01',
          periodEnd: '2026-12-31',
        },
      ],
    };
    component.ngOnChanges();
    component.workingBudgets[0].targetAmount = 250;
    component.newBudgets = [
      {
        id: null,
        costCentreId: 3,
        targetAmount: 500,
        periodStart: '2026-01-01',
        periodEnd: '2026-06-30',
      },
    ];
    const emitSpy = vi.spyOn(component.saveEvent, 'emit');

    component.onSave();

    expect(emitSpy).toHaveBeenCalledWith({
      team: component.team,
      budgetsToUpsert: [
        {
          id: 10,
          costCentreId: 2,
          targetAmount: 250,
          periodStart: '2026-01-01',
          periodEnd: '2026-12-31',
        },
        {
          id: null,
          costCentreId: 3,
          targetAmount: 500,
          periodStart: '2026-01-01',
          periodEnd: '2026-06-30',
        },
      ],
      budgetIdsToDelete: [],
    });
  });
});
