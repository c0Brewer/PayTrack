import { TestBed } from '@angular/core/testing';

import { BankStatementService } from './bank-statement-service';

describe('BankStatementService', () => {
  let service: BankStatementService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(BankStatementService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
