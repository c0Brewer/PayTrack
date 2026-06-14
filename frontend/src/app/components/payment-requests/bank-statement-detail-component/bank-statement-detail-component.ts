import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

import { DetailComponent } from '../../general/detail-component/detail-component';
import { BankStatementImportComponent } from '../bank-statement-import-component/bank-statement-import-component';

@Component({
  selector: 'app-bank-statement-detail-component',
  imports: [DetailComponent, BankStatementImportComponent],
  templateUrl: './bank-statement-detail-component.html',
  styleUrl: './bank-statement-detail-component.scss',
})
export class BankStatementDetailComponent implements OnInit {
  backLabel = 'Back';
  returnTo = '/';

  constructor(private readonly router: Router) {}

  ngOnInit(): void {
    const state = (this.router.getCurrentNavigation()?.extras.state ?? history.state) as {
      backLabel?: string;
      returnTo?: string;
    };

    if (typeof state.backLabel === 'string' && state.backLabel.trim()) {
      this.backLabel = state.backLabel;
    }

    if (typeof state.returnTo === 'string' && state.returnTo.trim()) {
      this.returnTo = state.returnTo;
    }
  }

  onBack(): void {
    this.router.navigateByUrl(this.returnTo);
  }
}
