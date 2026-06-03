import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CostCentreDto, SeasonDto, TeamDto } from '../../../types/exporter';

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
  const seasons: SeasonDto[] = [
    { id: 1, name: '2025', budgets: [] },
    { id: 2, name: '2026', budgets: [] },
  ];

  function clickAddBudgetButton(): void {
    const addButton = Array.from(
      fixture.nativeElement.querySelectorAll('button') as NodeListOf<HTMLButtonElement>,
    ).find((button) => button.textContent?.includes('Add Budget'));

    addButton?.click();
  }

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

  it('onDelete should emit deleteEvent for existing teams', () => {
    component.team = {
      id: 1,
      name: 'Platform',
      description: 'Builds product features',
      displayColor: '#2563eb',
      members: [],
      budgets: [],
    };
    const emitSpy = vi.spyOn(component.deleteEvent, 'emit');

    component.onDelete();

    expect(emitSpy).toHaveBeenCalledWith(component.team);
  });

  it('onDelete should not emit deleteEvent while creating a team', () => {
    component.team = {
      id: -1,
      name: 'New Team',
      description: '',
      displayColor: '#2563eb',
      members: [],
      budgets: [],
    };
    const emitSpy = vi.spyOn(component.deleteEvent, 'emit');

    component.onDelete();

    expect(emitSpy).not.toHaveBeenCalled();
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
          name: 'Existing budget',
          teamId: 1,
          costCentreId: 2,
          seasonId: 1,
          targetAmount: 100,
          periodStart: '2026-01-01',
          periodEnd: '2026-12-31',
          transactionIds: [],
          paidAmount: 0,
          approvedAmount: 0,
        },
      ],
    };
    component.ngOnChanges();
    component.toggleBudgetDeletion(component.workingBudgets[0]);
    component.newBudgets = [
      {
        id: null,
        name: 'New budget',
        costCentreId: 3,
        seasonId: 2,
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
          name: 'New budget',
          costCentreId: 3,
          seasonId: 2,
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
    component.seasons = seasons;
    component.newBudgetDraft = {
      id: null,
      name: 'New budget',
      costCentreId: 1,
      seasonId: 2,
      targetAmount: 500,
      periodStart: '2026-01-01',
      periodEnd: '2026-12-31',
    };

    component.addNewBudget();

    expect(component.newBudgets).toEqual([
      {
        id: null,
        name: 'New budget',
        costCentreId: 1,
        seasonId: 2,
        targetAmount: 500,
        periodStart: '2026-01-01',
        periodEnd: '2026-12-31',
      },
    ]);
    expect(component.newBudgetDraft).toEqual({
      id: null,
      name: '',
      description: null,
      costCentreId: 0,
      seasonId: 0,
      targetAmount: 0,
      periodStart: '',
      periodEnd: '',
    });

    component.removeNewBudget(0);

    expect(component.newBudgets).toEqual([]);
  });

  it('should mark missing budget fields red with messages', () => {
    component.costCentres = costCentres;
    fixture.detectChanges(false);

    clickAddBudgetButton();
    fixture.detectChanges(false);

    expect(component.newBudgets).toEqual([]);
    expect(component.hasBudgetFieldError('name')).toBe(true);
    expect(component.hasBudgetFieldError('costCentreId')).toBe(true);
    expect(component.hasBudgetFieldError('seasonId')).toBe(true);
    expect(component.hasBudgetFieldError('periodStart')).toBe(true);
    expect(component.hasBudgetFieldError('periodEnd')).toBe(true);
    expect(fixture.nativeElement.textContent).toContain('Name is required.');
    expect(fixture.nativeElement.textContent).toContain('Cost centre is required.');
    expect(fixture.nativeElement.textContent).toContain('Season is required.');
    expect(fixture.nativeElement.textContent).toContain('Period start is required.');
    expect(fixture.nativeElement.textContent).toContain('Period end is required.');

    const invalidControls = fixture.nativeElement.querySelectorAll('.input-error');
    expect(invalidControls.length).toBeGreaterThanOrEqual(5);
  });

  it('should mark a negative budget amount red with a message', () => {
    component.costCentres = costCentres;
    component.newBudgetDraft = {
      id: null,
      name: 'Budget',
      description: null,
      costCentreId: 1,
      seasonId: 1,
      targetAmount: -1,
      periodStart: '2026-01-01',
      periodEnd: '2026-12-31',
    };
    fixture.detectChanges(false);

    clickAddBudgetButton();
    fixture.detectChanges(false);

    const amountInput = fixture.nativeElement.querySelector(
      'input[type="number"]',
    ) as HTMLInputElement;

    expect(component.newBudgets).toEqual([]);
    expect(component.hasBudgetFieldError('targetAmount')).toBe(true);
    expect(fixture.nativeElement.textContent).toContain('Amount must be non-negative.');
    expect(amountInput.classList).toContain('input-error');
  });

  it('should mark a budget period end before the start red with a message', () => {
    component.costCentres = costCentres;
    component.newBudgetDraft = {
      id: null,
      name: 'Budget',
      description: null,
      costCentreId: 1,
      seasonId: 1,
      targetAmount: 500,
      periodStart: '2026-12-31',
      periodEnd: '2026-01-01',
    };
    fixture.detectChanges(false);

    clickAddBudgetButton();
    fixture.detectChanges(false);

    const dateInputs = fixture.nativeElement.querySelectorAll(
      'input[type="date"]',
    ) as NodeListOf<HTMLInputElement>;

    expect(component.newBudgets).toEqual([]);
    expect(component.hasBudgetFieldError('periodEnd')).toBe(true);
    expect(fixture.nativeElement.textContent).toContain(
      'Period end must not be before period start.',
    );
    expect(dateInputs[1].classList).toContain('input-error');
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
          name: 'Existing budget',
          teamId: 1,
          costCentreId: 2,
          seasonId: 1,
          targetAmount: 100,
          periodStart: '2026-01-01',
          periodEnd: '2026-12-31',
          transactionIds: [],
          paidAmount: 0,
          approvedAmount: 0,
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

  it('should resolve season names and fall back to the id', () => {
    component.seasons = seasons;

    expect(component.getSeasonName(2)).toBe('2026');
    expect(component.getSeasonName(99)).toBe('Season #99');
  });

  it('should label inactive cost centre options and prevent adding them to new budgets', () => {
    component.costCentres = costCentres;
    component.seasons = seasons;
    component.newBudgetDraft = {
      id: null,
      name: 'Blocked budget',
      costCentreId: 2,
      seasonId: 1,
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
    fixture.componentRef.setInput('seasons', seasons);
    fixture.detectChanges();

    const inactiveOption = Array.from(
      fixture.nativeElement.querySelectorAll('option') as NodeListOf<HTMLOptionElement>,
    ).find((option) => option.textContent?.trim() === 'Inactive Cost Centre (inactive)');

    expect(inactiveOption).toBeTruthy();
    expect(inactiveOption?.disabled).toBe(true);
  });

  it('should render seasons as select options', () => {
    fixture.componentRef.setInput('seasons', seasons);
    fixture.detectChanges();

    const seasonOption = Array.from(
      fixture.nativeElement.querySelectorAll('option') as NodeListOf<HTMLOptionElement>,
    ).find((option) => option.textContent?.trim() === '2026');

    expect(seasonOption).toBeTruthy();
  });
});
