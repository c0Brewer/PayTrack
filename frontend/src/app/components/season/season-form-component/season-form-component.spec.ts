import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SeasonFormComponent } from './season-form-component';

describe('SeasonFormComponent', () => {
  let component: SeasonFormComponent;
  let fixture: ComponentFixture<SeasonFormComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SeasonFormComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(SeasonFormComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('createSeason should emit trimmed season name and reset input', () => {
    const emitSpy = vi.spyOn(component.createSeasonEvent, 'emit');
    component.newSeasonName = '  2027  ';

    component.createSeason();

    expect(emitSpy).toHaveBeenCalledWith('2027');
    expect(component.newSeasonName).toBe('');
  });

  it('createSeason should do nothing for blank names', () => {
    const emitSpy = vi.spyOn(component.createSeasonEvent, 'emit');
    component.newSeasonName = '   ';

    component.createSeason();

    expect(emitSpy).not.toHaveBeenCalled();
    expect(component.newSeasonName).toBe('   ');
  });
});
