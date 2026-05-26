import { SlicePipe } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { CostCentreService } from '../../../services/cost-centre/cost-centre-service';
import { NotificationService } from '../../../services/notification/notification-service';
import { CostCentreDto } from '../../../types/exporter';
import { DetailComponent } from '../../general/detail-component/detail-component';

@Component({
  selector: 'app-cost-centre-detail-component',
  imports: [DetailComponent, SlicePipe],
  templateUrl: './cost-centre-detail-component.html',
  styleUrl: './cost-centre-detail-component.scss',
})
export class CostCentreDetailComponent implements OnInit {
  constructor(
    private readonly costCentreService: CostCentreService,
    private readonly notificationService: NotificationService,
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly cdr: ChangeDetectorRef,
  ) {}

  costCentre: CostCentreDto | null = null;

  ngOnInit(): void {
    this.route.paramMap.subscribe((params) => {
      const id = Number(params.get('id'));

      this.costCentreService.getCostCentre(id).subscribe({
        next: (data) => {
          this.costCentre = data;
          this.cdr.detectChanges();
        },
        error: (err: Error) => {
          this.notificationService.showError('Could not load cost centre: ' + err.message);
        },
      });
    });
  }

  goBack(): void {
    this.router.navigate(['/cost-centre']);
  }

  formatBudgetAmount(amount: number): string {
    return new Intl.NumberFormat('de-DE', { maximumFractionDigits: 2 }).format(amount);
  }
}
