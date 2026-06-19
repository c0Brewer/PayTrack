import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SeasonDto } from '../../../types/exporter';

import { SeasonListComponent } from './season-list-component';

const mockSeasons: SeasonDto[] = [
  { id: 1, name: '2025', isActive: true, budgets: [] },
  {
    id: 2,
    name: '2026',
    isActive: true,
    budgets: [{ id: 10 } as NonNullable<SeasonDto['budgets']>[number]],
  },
  {
    id: 3,
    name: '2024',
    isActive: false,
    budgets: [{ id: 20 } as NonNullable<SeasonDto['budgets']>[number]],
  },
];

describe('SeasonListComponent', () => {
  let component: SeasonListComponent;
  let fixture: ComponentFixture<SeasonListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SeasonListComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(SeasonListComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render season names and budget counts', () => {
    fixture.componentRef.setInput('seasons', mockSeasons);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('2025');
    expect(fixture.nativeElement.textContent).toContain('2026');
    expect(fixture.nativeElement.textContent).toContain('2024');
    expect(fixture.nativeElement.textContent).toContain('Inactive Seasons');
    expect(fixture.nativeElement.textContent).toContain('Reactivate');
    expect(fixture.nativeElement.textContent).toContain('1');
  });

  it('should render empty state when no seasons exist', () => {
    fixture.componentRef.setInput('seasons', []);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('No seasons found.');
  });

  it('should emit update event after editing a season name', () => {
    const updateSpy = vi.spyOn(component.updateSeason, 'emit');
    fixture.componentRef.setInput('seasons', mockSeasons);
    fixture.detectChanges();

    component.startEdit(mockSeasons[0]);
    component.editedSeasonName = '2025/26';
    component.submitEdit(mockSeasons[0]);

    expect(updateSpy).toHaveBeenCalledWith({ id: 1, name: '2025/26' });
  });

  it('should emit delete event for requested season', () => {
    const deleteSpy = vi.spyOn(component.deleteSeason, 'emit');

    component.requestDelete(mockSeasons[0]);

    expect(deleteSpy).toHaveBeenCalledWith(1);
  });

  it('should expose active and inactive seasons separately', () => {
    fixture.componentRef.setInput('seasons', mockSeasons);

    expect(component.visibleSeasons.map((season) => season.id)).toEqual([1, 2]);
    expect(component.inactiveSeasons.map((season) => season.id)).toEqual([3]);
  });

  it('should emit reactivate event for inactive season', () => {
    const reactivateSpy = vi.spyOn(component.reactivateSeason, 'emit');

    component.requestReactivate(mockSeasons[2]);

    expect(reactivateSpy).toHaveBeenCalledWith(3);
  });
});
