import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CostCentreDto, TeamDto } from '../../../types/exporter';

import { TeamEditModalComponent } from './team-edit-modal-component';

describe('TeamEditModalComponent', () => {
  let component: TeamEditModalComponent;
  let fixture: ComponentFixture<TeamEditModalComponent>;
  const costCentres: CostCentreDto[] = [
    {
      id: 1,
      name: 'Active Cost Centre',
      description: null,
      displayColor: null,
      budgets: [],
      isActive: true,
    },
    {
      id: 2,
      name: 'Inactive Cost Centre',
      description: null,
      displayColor: null,
      budgets: [],
      isActive: false,
    },
  ];

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

  it('selectedColor should return the configured hex color for valid values', () => {
    component.team.displayColor = '#2563eb';

    expect(component.selectedColor).toBe('#2563eb');
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

  it('getFieldError should allow valid name and description values', () => {
    component.team.name = 'Platform';
    component.team.description = 'Builds product features';

    expect(component.getFieldError('name')).toBe('');
    expect(component.getFieldError('description')).toBe('');
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

  it('onSave should include new budgets and existing budget deletions', () => {
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
    component.toggleBudgetDeletion(component.workingBudgets[0]);
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
          id: null,
          costCentreId: 3,
          targetAmount: 500,
          periodStart: '2026-01-01',
          periodEnd: '2026-06-30',
        },
      ],
      budgetIdsToDelete: [10],
    });
  });

  it('should add and remove new budgets with active cost centres', () => {
    component.costCentres = costCentres;
    component.newBudgetDraft = {
      id: null,
      costCentreId: 1,
      targetAmount: 500,
      periodStart: '2026-01-01',
      periodEnd: '2026-12-31',
    };

    component.addNewBudget();

    expect(component.newBudgets).toEqual([
      {
        id: null,
        costCentreId: 1,
        targetAmount: 500,
        periodStart: '2026-01-01',
        periodEnd: '2026-12-31',
      },
    ]);
    expect(component.newBudgetDraft).toEqual({
      id: null,
      costCentreId: 0,
      targetAmount: 0,
      periodStart: '',
      periodEnd: '',
    });

    component.removeNewBudget(0);

    expect(component.newBudgets).toEqual([]);
  });

  it('should mark existing budgets for deletion and restore them', () => {
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

    component.toggleBudgetDeletion(component.workingBudgets[0]);

    expect(component.workingBudgets[0].markedForDeletion).toBe(true);
    expect(component.hasTeamBeenChanged()).toBe(true);

    component.toggleBudgetDeletion(component.workingBudgets[0]);

    expect(component.workingBudgets[0].markedForDeletion).toBe(false);
  });

  it('should resolve cost centre names and fall back to the id', () => {
    component.costCentres = costCentres;

    expect(component.getCostCentreName(1)).toBe('Active Cost Centre');
    expect(component.getCostCentreName(99)).toBe('Cost Centre #99');
  });

  it('should label inactive cost centre options and prevent adding them to new budgets', () => {
    component.costCentres = costCentres;
    component.newBudgetDraft = {
      id: null,
      costCentreId: 2,
      targetAmount: 500,
      periodStart: '2026-01-01',
      periodEnd: '2026-12-31',
    };

    component.addNewBudget();

    expect(component.getCostCentreOptionLabel(costCentres[1])).toBe(
      'Inactive Cost Centre (inactive)',
    );
    expect(component.isCostCentreActive(1)).toBe(true);
    expect(component.isCostCentreActive(2)).toBe(false);
    expect(component.newBudgets).toEqual([]);
  });

  it('should leave active cost centre labels unchanged and treat unknown ids as selectable', () => {
    component.costCentres = costCentres;

    expect(component.getCostCentreOptionLabel(costCentres[0])).toBe('Active Cost Centre');
    expect(component.isCostCentreActive(999)).toBe(true);
  });

  it('should render inactive cost centres as disabled select options', () => {
    fixture.componentRef.setInput('costCentres', costCentres);
    fixture.detectChanges();

    const inactiveOption = Array.from(
      fixture.nativeElement.querySelectorAll('option') as NodeListOf<HTMLOptionElement>,
    ).find((option) => option.textContent?.trim() === 'Inactive Cost Centre (inactive)');

    expect(inactiveOption).toBeTruthy();
    expect(inactiveOption?.disabled).toBe(true);
  });
});
