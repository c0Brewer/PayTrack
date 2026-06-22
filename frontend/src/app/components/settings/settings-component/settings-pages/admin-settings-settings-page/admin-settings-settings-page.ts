import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { NotificationService } from '../../../../../services/notification/notification-service';
import { SystemSettingService } from '../../../../../services/system-setting/system-setting-service';
import type {
  CsvColumnSettingsDto,
  NotificationChannelGroupsDto,
  ReminderScheduleDto,
} from '../../../../../types/exporter';
import { BoxComponent } from '../../../../general/boxes/box-component/box-component';

@Component({
  selector: 'app-admin-settings-settings-page',
  imports: [BoxComponent, FormsModule],
  templateUrl: './admin-settings-settings-page.html',
  styleUrl: './admin-settings-settings-page.scss',
})
export class AdminSettingsSettingsPageComponent implements OnInit {
  csvSettings: CsvColumnSettingsDto = { nameColumn: 'Name', summeColumn: 'Summe' };
  private csvOriginal: CsvColumnSettingsDto = { nameColumn: 'Name', summeColumn: 'Summe' };
  csvLoading = false;
  csvSaving = false;

  get csvDirty(): boolean {
    return (
      this.csvSettings.nameColumn !== this.csvOriginal.nameColumn ||
      this.csvSettings.summeColumn !== this.csvOriginal.summeColumn
    );
  }

  notifSettings: NotificationChannelGroupsDto = {
    creation: { sendEmail: true, sendSlack: false, sendPush: true },
    confirmation: { sendEmail: true, sendSlack: false, sendPush: true },
    reminders: { sendEmail: true, sendSlack: false, sendPush: true },
    deletion: { sendEmail: true, sendSlack: false, sendPush: true },
    invoiceApproval: { sendEmail: true, sendSlack: false, sendPush: true },
    invoiceRejection: { sendEmail: true, sendSlack: false, sendPush: true },
    invoiceChangesRequested: { sendEmail: true, sendSlack: false, sendPush: true },
    invoicePaymentCompleted: { sendEmail: true, sendSlack: false, sendPush: true },
  };
  private notifOriginal: NotificationChannelGroupsDto = {
    creation: { sendEmail: true, sendSlack: false, sendPush: true },
    confirmation: { sendEmail: true, sendSlack: false, sendPush: true },
    reminders: { sendEmail: true, sendSlack: false, sendPush: true },
    deletion: { sendEmail: true, sendSlack: false, sendPush: true },
    invoiceApproval: { sendEmail: true, sendSlack: false, sendPush: true },
    invoiceRejection: { sendEmail: true, sendSlack: false, sendPush: true },
    invoiceChangesRequested: { sendEmail: true, sendSlack: false, sendPush: true },
    invoicePaymentCompleted: { sendEmail: true, sendSlack: false, sendPush: true },
  };
  notifLoading = false;
  notifSaving = false;

  get notifDirty(): boolean {
    return (
      this.notifSettings.creation.sendEmail !== this.notifOriginal.creation.sendEmail ||
      this.notifSettings.creation.sendSlack !== this.notifOriginal.creation.sendSlack ||
      this.notifSettings.creation.sendPush !== this.notifOriginal.creation.sendPush ||
      this.notifSettings.confirmation.sendEmail !== this.notifOriginal.confirmation.sendEmail ||
      this.notifSettings.confirmation.sendSlack !== this.notifOriginal.confirmation.sendSlack ||
      this.notifSettings.confirmation.sendPush !== this.notifOriginal.confirmation.sendPush ||
      this.notifSettings.reminders.sendEmail !== this.notifOriginal.reminders.sendEmail ||
      this.notifSettings.reminders.sendSlack !== this.notifOriginal.reminders.sendSlack ||
      this.notifSettings.reminders.sendPush !== this.notifOriginal.reminders.sendPush ||
      this.notifSettings.deletion.sendEmail !== this.notifOriginal.deletion.sendEmail ||
      this.notifSettings.deletion.sendSlack !== this.notifOriginal.deletion.sendSlack ||
      this.notifSettings.deletion.sendPush !== this.notifOriginal.deletion.sendPush ||
      this.notifSettings.invoiceApproval.sendEmail !== this.notifOriginal.invoiceApproval.sendEmail ||
      this.notifSettings.invoiceApproval.sendSlack !== this.notifOriginal.invoiceApproval.sendSlack ||
      this.notifSettings.invoiceApproval.sendPush !== this.notifOriginal.invoiceApproval.sendPush ||
      this.notifSettings.invoiceRejection.sendEmail !== this.notifOriginal.invoiceRejection.sendEmail ||
      this.notifSettings.invoiceRejection.sendSlack !== this.notifOriginal.invoiceRejection.sendSlack ||
      this.notifSettings.invoiceRejection.sendPush !== this.notifOriginal.invoiceRejection.sendPush ||
      this.notifSettings.invoiceChangesRequested.sendEmail !== this.notifOriginal.invoiceChangesRequested.sendEmail ||
      this.notifSettings.invoiceChangesRequested.sendSlack !== this.notifOriginal.invoiceChangesRequested.sendSlack ||
      this.notifSettings.invoiceChangesRequested.sendPush !== this.notifOriginal.invoiceChangesRequested.sendPush ||
      this.notifSettings.invoicePaymentCompleted.sendEmail !== this.notifOriginal.invoicePaymentCompleted.sendEmail ||
      this.notifSettings.invoicePaymentCompleted.sendSlack !== this.notifOriginal.invoicePaymentCompleted.sendSlack ||
      this.notifSettings.invoicePaymentCompleted.sendPush !== this.notifOriginal.invoicePaymentCompleted.sendPush
    );
  }

  reminderSettings: ReminderScheduleDto = {
    daysBeforeDue: [7, 2, 1],
    runAtHourUtc: 8,
    runAtMinuteUtc: 0,
    emailDelayMs: 500,
  };
  reminderDaysInput = '7, 2, 1';
  reminderTimeInput = '08:00';
  reminderEmailDelayInput = 500;
  private reminderOriginalDaysInput = '7, 2, 1';
  private reminderOriginalTime = '08:00';
  private reminderOriginalEmailDelay = 500;
  reminderLoading = false;
  reminderSaving = false;
  reminderDaysError = '';
  reminderTimeError = '';
  reminderEmailDelayError = '';

  get reminderDirty(): boolean {
    return (
      this.reminderDaysInput !== this.reminderOriginalDaysInput ||
      this.reminderTimeInput !== this.reminderOriginalTime ||
      this.reminderEmailDelayInput !== this.reminderOriginalEmailDelay
    );
  }

  private formatTime(hour: number, minute: number): string {
    return `${String(hour).padStart(2, '0')}:${String(minute).padStart(2, '0')}`;
  }

  private validateReminderDays(): void {
    const trimmed = this.reminderDaysInput.trim();
    if (trimmed === '') {
      this.reminderDaysError = '';
      return;
    }
    const parts = trimmed
      .split(',')
      .map((s) => s.trim())
      .filter((s) => s.length > 0);
    const valid = parts.every((s) => /^\d+$/.test(s) && parseInt(s, 10) >= 0);
    this.reminderDaysError = valid
      ? ''
      : 'Enter non-negative integers separated by commas, or leave empty to disable reminders.';
  }

  private validateReminderTime(): void {
    this.reminderTimeError = /^\d{2}:\d{2}$/.test(this.reminderTimeInput)
      ? ''
      : 'Please select a valid send time.';
  }

  private validateReminderEmailDelay(): void {
    const val = this.reminderEmailDelayInput;
    this.reminderEmailDelayError =
      Number.isInteger(val) && val >= 0 ? '' : 'Enter a non-negative whole number of milliseconds.';
  }

  onReminderDaysBlur(): void {
    this.validateReminderDays();
  }

  onReminderTimeBlur(): void {
    this.validateReminderTime();
  }

  onReminderEmailDelayBlur(): void {
    this.validateReminderEmailDelay();
  }

  constructor(
    private readonly systemSettingService: SystemSettingService,
    private readonly notificationService: NotificationService,
    private readonly cdr: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    this.loadCsvSettings();
    this.loadNotifSettings();
    this.loadReminderSettings();
  }

  loadCsvSettings(): void {
    this.csvLoading = true;
    this.systemSettingService.getCsvColumnSettings().subscribe({
      next: (data) => {
        this.csvSettings = { ...data };
        this.csvOriginal = { ...data };
        this.csvLoading = false;
        this.cdr.detectChanges();
      },
      error: (error: unknown) => {
        console.error('Failed to load CSV column settings', error);
        this.csvLoading = false;
        this.cdr.detectChanges();
      },
    });
  }

  saveCsvSettings(): void {
    this.csvSaving = true;
    this.systemSettingService.updateCsvColumnSettings(this.csvSettings).subscribe({
      next: () => {
        this.csvOriginal = { ...this.csvSettings };
        this.csvSaving = false;
        this.notificationService.showSuccess('CSV column settings saved.');
        this.cdr.detectChanges();
      },
      error: (error: unknown) => {
        this.csvSaving = false;
        this.notificationService.showError(
          error instanceof Error ? error.message : 'Failed to save CSV column settings.',
        );
        this.cdr.detectChanges();
      },
    });
  }

  loadNotifSettings(): void {
    this.notifLoading = true;
    this.systemSettingService.getNotificationChannelGroups().subscribe({
      next: (data) => {
        this.notifSettings = {
          creation: { ...data.creation },
          confirmation: { ...data.confirmation },
          reminders: { ...data.reminders },
          deletion: { ...data.deletion },
          invoiceApproval: { ...data.invoiceApproval },
          invoiceRejection: { ...data.invoiceRejection },
          invoiceChangesRequested: { ...data.invoiceChangesRequested },
          invoicePaymentCompleted: { ...data.invoicePaymentCompleted },
        };
        this.notifOriginal = {
          creation: { ...data.creation },
          confirmation: { ...data.confirmation },
          reminders: { ...data.reminders },
          deletion: { ...data.deletion },
          invoiceApproval: { ...data.invoiceApproval },
          invoiceRejection: { ...data.invoiceRejection },
          invoiceChangesRequested: { ...data.invoiceChangesRequested },
          invoicePaymentCompleted: { ...data.invoicePaymentCompleted },
        };
        this.notifLoading = false;
        this.cdr.detectChanges();
      },
      error: (error: unknown) => {
        console.error('Failed to load notification channel settings', error);
        this.notifLoading = false;
        this.cdr.detectChanges();
      },
    });
  }

  saveNotifSettings(): void {
    this.notifSaving = true;
    this.systemSettingService.updateNotificationChannelGroups(this.notifSettings).subscribe({
      next: () => {
        this.notifOriginal = {
          creation: { ...this.notifSettings.creation },
          confirmation: { ...this.notifSettings.confirmation },
          reminders: { ...this.notifSettings.reminders },
          deletion: { ...this.notifSettings.deletion },
          invoiceApproval: { ...this.notifSettings.invoiceApproval },
          invoiceRejection: { ...this.notifSettings.invoiceRejection },
          invoiceChangesRequested: { ...this.notifSettings.invoiceChangesRequested },
          invoicePaymentCompleted: { ...this.notifSettings.invoicePaymentCompleted },
        };
        this.notifSaving = false;
        this.notificationService.showSuccess('Notification channel settings saved.');
        this.cdr.detectChanges();
      },
      error: (error: unknown) => {
        this.notifSaving = false;
        this.notificationService.showError(
          error instanceof Error ? error.message : 'Failed to save notification channel settings.',
        );
        this.cdr.detectChanges();
      },
    });
  }

  loadReminderSettings(): void {
    this.reminderLoading = true;
    this.systemSettingService.getReminderSchedule().subscribe({
      next: (data) => {
        this.reminderSettings = { ...data };
        this.reminderDaysInput = data.daysBeforeDue.join(', ');
        this.reminderTimeInput = this.formatTime(data.runAtHourUtc, data.runAtMinuteUtc);
        this.reminderEmailDelayInput = data.emailDelayMs;
        this.reminderOriginalDaysInput = this.reminderDaysInput;
        this.reminderOriginalTime = this.reminderTimeInput;
        this.reminderOriginalEmailDelay = data.emailDelayMs;
        this.reminderDaysError = '';
        this.reminderTimeError = '';
        this.reminderEmailDelayError = '';
        this.reminderLoading = false;
        this.cdr.detectChanges();
      },
      error: (error: unknown) => {
        console.error('Failed to load reminder schedule', error);
        this.reminderLoading = false;
        this.cdr.detectChanges();
      },
    });
  }

  saveReminderSettings(): void {
    this.validateReminderDays();
    this.validateReminderTime();
    this.validateReminderEmailDelay();
    if (this.reminderDaysError || this.reminderTimeError || this.reminderEmailDelayError) return;

    const trimmedDays = this.reminderDaysInput.trim();
    const days =
      trimmedDays === '' ? [] : trimmedDays.split(',').map((s) => parseInt(s.trim(), 10));
    const [hourStr, minuteStr] = this.reminderTimeInput.split(':');
    const hour = parseInt(hourStr, 10);
    const minute = parseInt(minuteStr, 10);

    this.reminderSaving = true;
    this.systemSettingService
      .updateReminderSchedule({
        daysBeforeDue: days,
        runAtHourUtc: hour,
        runAtMinuteUtc: minute,
        emailDelayMs: this.reminderEmailDelayInput,
      })
      .subscribe({
        next: () => {
          this.reminderSettings.daysBeforeDue = days;
          this.reminderSettings.runAtHourUtc = hour;
          this.reminderSettings.runAtMinuteUtc = minute;
          this.reminderSettings.emailDelayMs = this.reminderEmailDelayInput;
          this.reminderOriginalDaysInput = this.reminderDaysInput;
          this.reminderOriginalTime = this.reminderTimeInput;
          this.reminderOriginalEmailDelay = this.reminderEmailDelayInput;
          this.reminderSaving = false;
          this.notificationService.showSuccess('Reminder schedule saved.');
          this.cdr.detectChanges();
        },
        error: (error: unknown) => {
          this.reminderSaving = false;
          this.notificationService.showError(
            error instanceof Error ? error.message : 'Failed to save reminder schedule.',
          );
          this.cdr.detectChanges();
        },
      });
  }
}
