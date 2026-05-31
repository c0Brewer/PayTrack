import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BankStatementImportComponent } from './bank-statement-import-component';

describe('BankStatementImportComponent', () => {
  let component: BankStatementImportComponent;
  let fixture: ComponentFixture<BankStatementImportComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BankStatementImportComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(BankStatementImportComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
