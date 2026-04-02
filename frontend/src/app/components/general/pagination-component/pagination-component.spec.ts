import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { vi } from 'vitest';

import { PaginationComponent } from './pagination-component';

describe('PaginationComponent', () => {
  let component: PaginationComponent;
  let fixture: ComponentFixture<PaginationComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PaginationComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(PaginationComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should emit next event when onNext is called', () => {
    const spy = vi.spyOn(component.next, 'emit');
    component.onNext();
    expect(spy).toHaveBeenCalled();
  });

  it('should emit prev event when onPrev is called', () => {
    const spy = vi.spyOn(component.prev, 'emit');
    component.onPrev();
    expect(spy).toHaveBeenCalled();
  });

  it('should have default input values', () => {
    expect(component.hasNext).toBe(false);
    expect(component.hasPrev).toBe(false);
    expect(component.currentPage).toBe(1);
    expect(component.maxPage).toBe(1);
  });

  it('should allow setting input values', () => {
    component.hasNext = true;
    component.hasPrev = true;
    component.currentPage = 3;
    component.maxPage = 5;

    expect(component.hasNext).toBe(true);
    expect(component.hasPrev).toBe(true);
    expect(component.currentPage).toBe(3);
    expect(component.maxPage).toBe(5);
  });

  it('should call onPrev when left button is clicked', () => {
    const spy = vi.spyOn(component, 'onPrev');

    const leftButton = fixture.debugElement.query(By.css('button.left'));
    leftButton.nativeElement.click();

    expect(spy).toHaveBeenCalled();
  });

  it('should call onNext when right button is clicked', () => {
    const spy = vi.spyOn(component, 'onNext');

    const rightButton = fixture.debugElement.query(By.css('button.right'));
    rightButton.nativeElement.click();

    expect(spy).toHaveBeenCalled();
  });
});
