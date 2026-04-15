import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ReceiptOverviewComponent } from './receipt-overview-component';

describe('ReceiptOverviewComponent', () => {
  let component: ReceiptOverviewComponent;
  let fixture: ComponentFixture<ReceiptOverviewComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ReceiptOverviewComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(ReceiptOverviewComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
