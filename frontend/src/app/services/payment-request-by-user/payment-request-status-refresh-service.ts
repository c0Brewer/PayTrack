import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class PaymentRequestStatusRefreshService {
  private readonly refreshRequested = new Subject<void>();

  readonly refreshRequested$ = this.refreshRequested.asObservable();

  requestRefresh(): void {
    this.refreshRequested.next();
  }
}
