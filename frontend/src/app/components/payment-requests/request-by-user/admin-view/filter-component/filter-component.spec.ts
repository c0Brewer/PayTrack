import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';

import { TeamService } from '../../../../../services/team/team-service';
import { UserService } from '../../../../../services/user/user-service';

import { AdminInvoiceFilterComponent } from './filter-component';

describe('AdminInvoiceFilterComponent', () => {
  let component: AdminInvoiceFilterComponent;
  let fixture: ComponentFixture<AdminInvoiceFilterComponent>;

  const teamServiceMock = {
    getTeams: vi.fn().mockReturnValue(of({ items: [], totalCount: 0, limit: 1000, offset: 0 })),
  };
  const userServiceMock = {
    getUser: vi.fn().mockReturnValue(of({ items: [], totalCount: 0, limit: 1000, offset: 0 })),
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AdminInvoiceFilterComponent],
      providers: [
        { provide: TeamService, useValue: teamServiceMock },
        { provide: UserService, useValue: userServiceMock },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AdminInvoiceFilterComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load teams on init', () => {
    expect(teamServiceMock.getTeams).toHaveBeenCalled();
    expect(component.teams).toEqual([]);
  });

  it('should load users on init', () => {
    expect(userServiceMock.getUser).toHaveBeenCalled();
    expect(component.users).toEqual([]);
  });

  it('should emit filter options via updateFilter output', () => {
    let emitted: unknown;
    component.updateFilter.subscribe((f) => (emitted = f));

    component.emitFilter();

    expect(emitted).toBeDefined();
  });

  it('should include status in filter options when filterStatus is set', () => {
    component.filterStatus = 2;

    const options = component.getFilterOptions();

    expect(options!.Status as number).toBe(2);
  });
});
