import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ReceiptSubmitComponent } from './receipt-submit-component';

describe('ReceiptSubmitComponent', () => {
  let component: ReceiptSubmitComponent;
  let fixture: ComponentFixture<ReceiptSubmitComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ReceiptSubmitComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(ReceiptSubmitComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
