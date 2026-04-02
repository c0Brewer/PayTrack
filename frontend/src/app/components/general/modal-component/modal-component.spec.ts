import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ModalComponent } from './modal-component';

describe('ModalComponent', () => {
  let component: ModalComponent;
  let fixture: ComponentFixture<ModalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ModalComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(ModalComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should emit close event when onClose is called', () => {
    const spy = vi.spyOn(component.closeEvent, 'emit');
    component.onClose();
    expect(spy).toHaveBeenCalled();
  });
});
