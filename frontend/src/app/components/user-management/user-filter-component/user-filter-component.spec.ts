import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UserFilterComponent } from './user-filter-component';

describe('UserFilterComponent', () => {
  let component: UserFilterComponent;
  let fixture: ComponentFixture<UserFilterComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UserFilterComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(UserFilterComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
