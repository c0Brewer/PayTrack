import { ComponentFixture, TestBed } from '@angular/core/testing';

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
});
