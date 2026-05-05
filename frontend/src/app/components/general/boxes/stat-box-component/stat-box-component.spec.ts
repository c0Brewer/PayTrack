import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';

import { StatBoxComponent } from './stat-box-component';

describe('StatBoxComponent', () => {
  let fixture: ComponentFixture<StatBoxComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [StatBoxComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(StatBoxComponent);
  });

  it('should create', () => {
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should render header, content, and icon', () => {
    fixture.componentRef.setInput('header', 'Gesamt Teams');
    fixture.componentRef.setInput('content', 10);
    fixture.componentRef.setInput('icon', 'groups');
    fixture.detectChanges();

    expect(
      fixture.debugElement.query(By.css('.stat-box__header')).nativeElement.textContent,
    ).toContain('Gesamt Teams');
    expect(
      fixture.debugElement.query(By.css('.stat-box__value')).nativeElement.textContent,
    ).toContain('10');
    expect(
      fixture.debugElement.query(By.css('.material-symbols-outlined')).nativeElement.textContent,
    ).toContain('groups');
  });

  it('should allow overriding the icon colors', () => {
    fixture.componentRef.setInput('icon', 'groups');
    fixture.componentRef.setInput('iconColor', '#2563eb');
    fixture.componentRef.setInput('iconBackgroundColor', '#dbeafe');
    fixture.detectChanges();

    const iconBox = fixture.debugElement.query(By.css('.stat-box__icon')).nativeElement;

    expect(iconBox.style.color).toBe('rgb(37, 99, 235)');
    expect(iconBox.style.backgroundColor).toBe('rgb(219, 234, 254)');
  });
});
