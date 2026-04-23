import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormsModule } from '@angular/forms';
import { vi } from 'vitest';

import { Role, GetUserOptions } from '../../../types/exporter';

import { UserFilterComponent } from './user-filter-component';

describe('UserFilterComponent', () => {
  let component: UserFilterComponent;
  let fixture: ComponentFixture<UserFilterComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UserFilterComponent, FormsModule],
    }).compileComponents();

    fixture = TestBed.createComponent(UserFilterComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    // Enable fake timers for debounce tests
    vi.useFakeTimers();
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should emit updated filter on name change after debounce', async () => {
    const spy = vi.spyOn(component.updateFilter, 'emit');

    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    component.onNameFilterChange({ target: { value: 'Alice' } } as any);

    // advance timers by 400ms to trigger debounce
    vi.advanceTimersByTime(400);

    expect(spy).toHaveBeenCalledWith(expect.objectContaining({ Name: 'Alice' }));
  });

  it('should emit updated filter on email change after debounce', async () => {
    const spy = vi.spyOn(component.updateFilter, 'emit');

    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    component.onEmailFilterChange({ target: { value: 'a@test.com' } } as any);

    vi.advanceTimersByTime(400);

    expect(spy).toHaveBeenCalledWith(expect.objectContaining({ Email: 'a@test.com' }));
  });

  it('should emit updated filter on role change after debounce', async () => {
    const spy = vi.spyOn(component.updateFilter, 'emit');

    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    component.onRoleFilterChange({ target: { value: Role.ADMIN.toString() } } as any);

    vi.advanceTimersByTime(100);

    expect(spy).toHaveBeenCalledWith(expect.objectContaining({ Role: Role.ADMIN }));
  });

  it('should emit updated filter on isActive change after debounce', async () => {
    const spy = vi.spyOn(component.updateFilter, 'emit');

    component.filterIsActive = true;
    component.onIsActiveFilterChange();

    vi.advanceTimersByTime(100);

    expect(spy).toHaveBeenCalledWith(expect.objectContaining({ IsActive: true }));
  });

  it('should emit limit change correctly', () => {
    const spy = vi.spyOn(component.limitChange, 'emit');

    component.limit = 50;
    component.onLimitChange();

    expect(spy).toHaveBeenCalledWith(50);
  });

  it('getGetUserOptions should return proper object', () => {
    component.filterName = 'Alice';
    component.filterEmail = 'a@test.com';
    component.filterRole = Role.TEAM_LEAD;
    component.filterIsActive = true;

    const options: GetUserOptions = component.getGetUserOptions();

    expect(options).toEqual({
      Name: 'Alice',
      Email: 'a@test.com',
      Role: Role.TEAM_LEAD,
      IsActive: true,
      IncludeTeam: undefined,
      Limit: undefined,
      Offset: undefined,
    });
  });
});
