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

  it('onSave should emit saveEvent if team changed', () => {
    component.team.name = 'Platform';
    component.originalTeam = { ...component.team, name: 'Operations' };
    const emitSpy = vi.spyOn(component.saveEvent, 'emit');

    component.onSave();

    expect(emitSpy).toHaveBeenCalledWith(component.team);
  });

  it('onSave should close the modal if team is unchanged', () => {
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
});
