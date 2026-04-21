import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TeamFilterComponent } from './team-filter-component';

describe('TeamFilterComponent', () => {
  let component: TeamFilterComponent;
  let fixture: ComponentFixture<TeamFilterComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TeamFilterComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(TeamFilterComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
