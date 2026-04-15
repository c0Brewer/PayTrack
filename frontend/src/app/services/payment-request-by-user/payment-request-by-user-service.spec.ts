import { TestBed } from '@angular/core/testing';

import { PaymentRequestByUserService } from './payment-request-by-user-service';

describe('PaymentRequestByUserService', () => {
  let service: PaymentRequestByUserService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(PaymentRequestByUserService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
