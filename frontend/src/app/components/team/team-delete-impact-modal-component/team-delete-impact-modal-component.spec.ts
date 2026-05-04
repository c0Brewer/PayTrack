import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TeamDeleteImpactModalComponent } from './team-delete-impact-modal-component';

describe('TeamDeleteImpactModalComponent', () => {
  let component: TeamDeleteImpactModalComponent;
  let fixture: ComponentFixture<TeamDeleteImpactModalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TeamDeleteImpactModalComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(TeamDeleteImpactModalComponent);
    component = fixture.componentInstance;
  });

  it('should show deactivate controls for active teams with impact', () => {
    component.impact = {
      teamId: 1,
      teamName: 'Platform',
      canDelete: false,
      affectedUserCount: 1,
      blockingBudgetCount: 1,
      blockingTransactionCount: 0,
      invoiceCount: 0,
      warningMessage: '',
    };
    component.isTeamActive = true;
    fixture.detectChanges();

    expect(component.hasImpact).toBe(true);
    expect(component.isReadOnlyImpact).toBe(false);
    expect(fixture.nativeElement.textContent).toContain('Deactivate');
    expect(fixture.nativeElement.textContent).toContain('Cancel');
    expect(fixture.nativeElement.textContent).not.toContain('Back');
  });

  it('should show only back for inactive teams with impact', () => {
    component.impact = {
      teamId: 1,
      teamName: 'Platform',
      canDelete: false,
      affectedUserCount: 1,
      blockingBudgetCount: 1,
      blockingTransactionCount: 0,
      invoiceCount: 0,
      warningMessage: '',
    };
    component.isTeamActive = false;
    fixture.detectChanges();

    expect(component.isReadOnlyImpact).toBe(true);
    expect(fixture.nativeElement.textContent).toContain('already inactive');
    expect(fixture.nativeElement.textContent).toContain('Back');
    expect(fixture.nativeElement.textContent).not.toContain('Deactivate');
    expect(fixture.nativeElement.textContent).not.toContain('Cancel');
  });

  it('should still show delete controls for inactive teams without impact', () => {
    component.impact = {
      teamId: 1,
      teamName: 'Platform',
      canDelete: true,
      affectedUserCount: 0,
      blockingBudgetCount: 0,
      blockingTransactionCount: 0,
      invoiceCount: 0,
      warningMessage: '',
    };
    component.isTeamActive = false;
    fixture.detectChanges();

    expect(component.hasImpact).toBe(false);
    expect(component.isReadOnlyImpact).toBe(false);
    expect(fixture.nativeElement.textContent).toContain('Delete');
    expect(fixture.nativeElement.textContent).toContain('Cancel');
  });

  it('should emit closeEvent when back is clicked', () => {
    component.impact = {
      teamId: 1,
      teamName: 'Platform',
      canDelete: false,
      affectedUserCount: 1,
      blockingBudgetCount: 0,
      blockingTransactionCount: 0,
      invoiceCount: 0,
      warningMessage: '',
    };
    component.isTeamActive = false;
    const closeSpy = vi.spyOn(component.closeEvent, 'emit');
    fixture.detectChanges();

    const backButton = fixture.nativeElement.querySelector('.modal-actions button');
    backButton.click();

    expect(closeSpy).toHaveBeenCalledOnce();
  });
});
