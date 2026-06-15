import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SeasonDto } from '../../../types/exporter';

import { SeasonListComponent } from './season-list-component';

const mockSeasons: SeasonDto[] = [
  { id: 1, name: '2025', budgets: [] },
  { id: 2, name: '2026', budgets: [{ id: 10 } as NonNullable<SeasonDto['budgets']>[number]] },
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
    expect(fixture.nativeElement.textContent).toContain('1');
  });

  it('should render empty state when no seasons exist', () => {
    fixture.componentRef.setInput('seasons', []);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('No seasons found.');
  });
});
