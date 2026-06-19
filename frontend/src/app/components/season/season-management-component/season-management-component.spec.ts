//AI helped with the test cases

import { ChangeDetectorRef } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';

import { NotificationService } from '../../../services/notification/notification-service';
import { SeasonService } from '../../../services/season/season-service';
import { SeasonDto } from '../../../types/exporter';

import { SeasonManagementComponent } from './season-management-component';

const mockSeasons: SeasonDto[] = [
  { id: 1, name: '2025', isActive: true, budgets: [] },
  {
    id: 2,
    name: '2026',
    isActive: true,
    budgets: [{ id: 10 } as NonNullable<SeasonDto['budgets']>[number]],
  },
  { id: 3, name: '2024', isActive: false, budgets: [] },
];

describe('SeasonManagementComponent', () => {
  let component: SeasonManagementComponent;
  let fixture: ComponentFixture<SeasonManagementComponent>;
  let seasonServiceMock: {
    getSeasons: ReturnType<typeof vi.fn>;
    createSeason: ReturnType<typeof vi.fn>;
    updateSeason: ReturnType<typeof vi.fn>;
    deleteSeason: ReturnType<typeof vi.fn>;
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
      updateSeason: vi.fn().mockReturnValue(of(mockSeasons[0])),
      deleteSeason: vi.fn().mockReturnValue(of(null)),
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

    expect(seasonServiceMock.getSeasons).toHaveBeenCalledWith({ IncludeInactive: true });
    expect(component.seasons).toEqual(mockSeasons);
  });

  it('loadSeasons should show error when API throws', () => {
    seasonServiceMock.getSeasons.mockReturnValueOnce(throwError(() => new Error('API error')));

    component.loadSeasons();

    expect(notificationServiceMock.showError).toHaveBeenCalledWith(
      'Could not load seasons: API error',
    );
  });

  it('createSeason should create a season and reload', () => {
    const loadSpy = vi.spyOn(component, 'loadSeasons');

    component.createSeason('2027');

    expect(seasonServiceMock.createSeason).toHaveBeenCalledWith({ name: '2027' });
    expect(notificationServiceMock.showSuccess).toHaveBeenCalledWith('Season created successfully');
    expect(loadSpy).toHaveBeenCalledOnce();
  });

  it('createSeason should show error when API throws', () => {
    seasonServiceMock.createSeason.mockReturnValueOnce(
      throwError(() => new Error('Create failed')),
    );

    component.createSeason('2027');

    expect(notificationServiceMock.showError).toHaveBeenCalledWith(
      'Could not create season: Create failed',
    );
  });

  it('createSeason should show duplicate-name message without prefix', () => {
    seasonServiceMock.createSeason.mockReturnValueOnce(
      throwError(() => new Error('season name already taken')),
    );

    component.createSeason('2026');

    expect(notificationServiceMock.showError).toHaveBeenCalledWith('season name already taken');
  });

  it('updateSeason should update a season and reload', () => {
    const loadSpy = vi.spyOn(component, 'loadSeasons');

    component.updateSeason({ id: 1, name: '2025/26' });

    expect(seasonServiceMock.updateSeason).toHaveBeenCalledWith(1, { name: '2025/26' });
    expect(notificationServiceMock.showSuccess).toHaveBeenCalledWith('Season updated successfully');
    expect(loadSpy).toHaveBeenCalledOnce();
  });

  it('updateSeason should show error when API throws', () => {
    seasonServiceMock.updateSeason.mockReturnValueOnce(
      throwError(() => new Error('Update failed')),
    );

    component.updateSeason({ id: 1, name: '2025/26' });

    expect(notificationServiceMock.showError).toHaveBeenCalledWith(
      'Could not update season: Update failed',
    );
  });

  it('updateSeason should show duplicate-name message without prefix', () => {
    seasonServiceMock.updateSeason.mockReturnValueOnce(
      throwError(() => new Error('season name already taken')),
    );

    component.updateSeason({ id: 1, name: '2026' });

    expect(notificationServiceMock.showError).toHaveBeenCalledWith('season name already taken');
  });

  it('reactivateSeason should update active flag and reload', () => {
    const loadSpy = vi.spyOn(component, 'loadSeasons');

    component.reactivateSeason(3);

    expect(seasonServiceMock.updateSeason).toHaveBeenCalledWith(3, { isActive: true });
    expect(notificationServiceMock.showSuccess).toHaveBeenCalledWith(
      'Season reactivated successfully',
    );
    expect(loadSpy).toHaveBeenCalledOnce();
  });

  it('reactivateSeason should show error when API throws', () => {
    seasonServiceMock.updateSeason.mockReturnValueOnce(
      throwError(() => new Error('Reactivate failed')),
    );

    component.reactivateSeason(3);

    expect(notificationServiceMock.showError).toHaveBeenCalledWith(
      'Could not reactivate season: Reactivate failed',
    );
  });

  it('openDeleteSeason should select the season for delete modal', () => {
    component.seasons = mockSeasons;

    component.openDeleteSeason(1);

    expect(component.seasonToDelete).toEqual(mockSeasons[0]);
  });

  it('closeDeleteSeasonModal should clear selected season', () => {
    component.seasonToDelete = mockSeasons[0];

    component.closeDeleteSeasonModal();

    expect(component.seasonToDelete).toBeNull();
  });

  it('selectedSeasonHasDependencies should detect linked budgets', () => {
    component.seasonToDelete = mockSeasons[1];

    expect(component.selectedSeasonHasDependencies).toBe(true);

    component.seasonToDelete = mockSeasons[0];
    expect(component.selectedSeasonHasDependencies).toBe(false);
  });

  it('confirmDeleteSeason should delete and reload', () => {
    const loadSpy = vi.spyOn(component, 'loadSeasons');

    component.seasonToDelete = mockSeasons[0];
    component.confirmDeleteSeason();

    expect(seasonServiceMock.deleteSeason).toHaveBeenCalledWith(1);
    expect(notificationServiceMock.showSuccess).toHaveBeenCalledWith('Season deleted successfully');
    expect(component.seasonToDelete).toBeNull();
    expect(loadSpy).toHaveBeenCalledOnce();
  });

  it('confirmDeleteSeason should show deactivated message when service returns season', () => {
    seasonServiceMock.deleteSeason.mockReturnValueOnce(of({ ...mockSeasons[1], isActive: false }));

    component.seasonToDelete = mockSeasons[1];
    component.confirmDeleteSeason();

    expect(notificationServiceMock.showSuccess).toHaveBeenCalledWith(
      'Season deactivated successfully',
    );
  });

  it('confirmDeleteSeason should do nothing without a selected season', () => {
    component.seasonToDelete = null;
    component.confirmDeleteSeason();

    expect(seasonServiceMock.deleteSeason).not.toHaveBeenCalled();
  });

  it('confirmDeleteSeason should show error when API throws', () => {
    seasonServiceMock.deleteSeason.mockReturnValueOnce(
      throwError(() => new Error('Delete failed')),
    );

    component.seasonToDelete = mockSeasons[0];
    component.confirmDeleteSeason();

    expect(notificationServiceMock.showError).toHaveBeenCalledWith(
      'Could not delete season: Delete failed',
    );
  });

  it('should pass seasons to the season list component', () => {
    component.seasons = mockSeasons;
    fixture.detectChanges();

    const list = fixture.nativeElement.querySelector('app-season-list-component');

    expect(list).not.toBeNull();
  });
});
