import { Injectable, isDevMode } from '@angular/core';
import { SwUpdate } from '@angular/service-worker';
import { interval } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class AppUpdateService {
  private static readonly updateCheckIntervalMs = 5 * 60 * 1000;
  private reloadInProgress = false;

  constructor(private readonly swUpdate: SwUpdate) {}

  init(): void {
    if (isDevMode() || !this.swUpdate.isEnabled) {
      return;
    }

    this.swUpdate.versionUpdates.subscribe((event) => {
      if (event.type === 'VERSION_READY') {
        void this.activateAndReload();
      }
    });

    this.swUpdate.unrecoverable.subscribe(() => {
      this.reload();
    });

    void this.checkForUpdate();
    interval(AppUpdateService.updateCheckIntervalMs).subscribe(() => {
      void this.checkForUpdate();
    });
  }

  private async checkForUpdate(): Promise<void> {
    try {
      const updateAvailable = await this.swUpdate.checkForUpdate();
      if (updateAvailable) {
        await this.activateAndReload();
      }
    } catch (error) {
      console.error('Failed to check for application update', error);
    }
  }

  private async activateAndReload(): Promise<void> {
    if (this.reloadInProgress) {
      return;
    }

    this.reloadInProgress = true;
    try {
      await this.swUpdate.activateUpdate();
    } catch (error) {
      console.error('Failed to activate application update', error);
    } finally {
      this.reload();
    }
  }

  private reload(): void {
    window.location.reload();
  }
}
