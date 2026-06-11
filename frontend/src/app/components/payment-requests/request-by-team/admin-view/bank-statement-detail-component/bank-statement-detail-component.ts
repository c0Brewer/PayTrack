import { Component } from '@angular/core';
import { Router } from '@angular/router';

import { BankStatementImportComponent } from '../../../bank-statement-import-component/bank-statement-import-component';
import { DetailComponent } from '../../../../general/detail-component/detail-component';

@Component({
  selector: 'app-team-request-bank-statement-detail-component',
  imports: [DetailComponent, BankStatementImportComponent],
  templateUrl: './bank-statement-detail-component.html',
  styleUrl: './bank-statement-detail-component.scss',
})
export class TeamRequestBankStatementDetailComponent {
  constructor(private readonly router: Router) {}

  onBack(): void {
    this.router.navigate(['/payment-requests-by-team']);
  }
}
