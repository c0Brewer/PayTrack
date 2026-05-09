//AI helped with the test cases

import { ChangeDetectorRef } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';

import { NotificationService } from '../../../services/notification/notification-service';
import { SeasonService } from '../../../services/season/season-service';
import { SeasonDto } from '../../../types/exporter';

import { SeasonManagementComponent } from './season-management-component';

const mockSeasons: SeasonDto[] = [
  { id: 1, name: '2025', budgets: [] },
  { id: 2, name: '2026', budgets: [{ id: 10 } as NonNullable<SeasonDto['budgets']>[number]] },
];

describe('SeasonManagementComponent', () => {
  let component: SeasonManagementComponent;
  let fixture: ComponentFixture<SeasonManagementComponent>;
  let seasonServiceMock: {
    getSeasons: ReturnType<typeof vi.fn>;
    createSeason: ReturnType<typeof vi.fn>;
  };
  let notificationServiceMock: {
    showError: ReturnType<typeof vi.fn>;
    showSuccess: ReturnType<typeof vi.fn>;
  };
  let cdrMock: { markForCheck: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    seasonServiceMock = {
      getSeasons: vi.fn().mockReturnValue(of(mockSeasons)),
      createSeason: vi.fn().mockReturnValue(of(mockSeasons[0])),
    };
    notificationServiceMock = {
      showError: vi.fn(),
      showSuccess: vi.fn(),
    };
    cdrMock = { markForCheck: vi.fn() };

    await TestBed.configureTestingModule({
      imports: [SeasonManagementComponent],
      providers: [
        { provide: SeasonService, useValue: seasonServiceMock },
        { provide: NotificationService, useValue: notificationServiceMock },
        { provide: ChangeDetectorRef, useValue: cdrMock },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(SeasonManagementComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('ngOnInit should load seasons', () => {
    component.ngOnInit();

    expect(seasonServiceMock.getSeasons).toHaveBeenCalled();
    expect(component.seasons).toEqual(mockSeasons);
  });

  it('loadSeasons should show error when API throws', () => {
    seasonServiceMock.getSeasons.mockReturnValueOnce(throwError(() => new Error('API error')));

    component.loadSeasons();

    expect(notificationServiceMock.showError).toHaveBeenCalledWith(
      'Could not load seasons: API error',
    );
  });

  it('createSeason should trim input, create a season, reset the form, and reload', () => {
    const loadSpy = vi.spyOn(component, 'loadSeasons');
    component.newSeasonName = '  2027  ';

    component.createSeason();

    expect(seasonServiceMock.createSeason).toHaveBeenCalledWith({ name: '2027' });
    expect(notificationServiceMock.showSuccess).toHaveBeenCalledWith(
      'Season created successfully',
    );
    expect(component.newSeasonName).toBe('');
    expect(loadSpy).toHaveBeenCalledOnce();
  });

  it('createSeason should do nothing for blank names', () => {
    component.newSeasonName = '   ';

    component.createSeason();

    expect(seasonServiceMock.createSeason).not.toHaveBeenCalled();
  });

  it('createSeason should show error when API throws', () => {
    seasonServiceMock.createSeason.mockReturnValueOnce(
      throwError(() => new Error('Create failed')),
    );
    component.newSeasonName = '2027';

    component.createSeason();

    expect(notificationServiceMock.showError).toHaveBeenCalledWith(
      'Could not create season: Create failed',
    );
  });

  it('should render season names and budget counts', () => {
    component.seasons = mockSeasons;
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('2025');
    expect(fixture.nativeElement.textContent).toContain('2026');
    expect(fixture.nativeElement.textContent).toContain('1');
  });
});
