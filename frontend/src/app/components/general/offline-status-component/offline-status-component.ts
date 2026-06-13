import { Component, inject } from '@angular/core';

import { OfflineService } from '../../../services/offline/offline-service';

@Component({
  selector: 'app-offline-status-component',
  imports: [],
  templateUrl: './offline-status-component.html',
  styleUrl: './offline-status-component.scss',
})
export class OfflineStatusComponent {
  protected readonly offlineService = inject(OfflineService);
}
