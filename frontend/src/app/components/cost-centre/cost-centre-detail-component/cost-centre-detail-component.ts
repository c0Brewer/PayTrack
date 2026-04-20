import { SlicePipe } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { CostCentreService } from '../../../services/cost-centre/cost-centre-service';
import { NotificationService } from '../../../services/notification/notification-service';
import { CostCentreDto } from '../../../types/exporter';

@Component({
  selector: 'app-cost-centre-detail-component',
  imports: [SlicePipe],
  templateUrl: './cost-centre-detail-component.html',
  styleUrl: './cost-centre-detail-component.scss',
})
export class CostCentreDetailComponent implements OnInit {
  constructor(
    private readonly costCentreService: CostCentreService,
    private readonly notificationService: NotificationService,
    private readonly route: ActivatedRoute,
    private readonly router: Router,
  ) {}

  costCentre: CostCentreDto | null = null;

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.costCentreService.getCostCentre(id).subscribe({
      next: (data) => {
        this.costCentre = data;
      },
      error: (err: Error) => {
        this.notificationService.showError('Could not load cost centre: ' + err.message);
      },
    });
  }

  goBack(): void {
    this.router.navigate(['/cost-centre']);
  }
}
