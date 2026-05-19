import { ComponentFixture, TestBed } from '@angular/core/testing';
import { vi } from 'vitest';

import { DetailComponent } from './detail-component';

describe('DetailComponent', () => {
  let component: DetailComponent;
  let fixture: ComponentFixture<DetailComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DetailComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(DetailComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render the configured title and subtitle', () => {
    fixture.componentRef.setInput('title', 'Aerodynamics');
    fixture.componentRef.setInput('subtitle', 'Cost centre details');
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('h1').textContent).toContain('Aerodynamics');
    expect(fixture.nativeElement.querySelector('.detail-shell__subtitle').textContent).toContain(
      'Cost centre details',
    );
  });

  it('should emit backClick when the back button is clicked', () => {
    const spy = vi.spyOn(component.backClick, 'emit');
    fixture.componentRef.setInput('backLabel', 'Back');
    fixture.detectChanges();

    fixture.nativeElement.querySelector('.detail-shell__back').click();

    expect(spy).toHaveBeenCalledOnce();
  });
});
