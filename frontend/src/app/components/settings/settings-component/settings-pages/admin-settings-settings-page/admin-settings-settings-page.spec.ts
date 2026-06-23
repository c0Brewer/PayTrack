//AI helped with the test cases

import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';

import { NotificationService } from '../../../../../services/notification/notification-service';
import { SystemSettingService } from '../../../../../services/system-setting/system-setting-service';

import { AdminSettingsSettingsPageComponent } from './admin-settings-settings-page';

const mockSystemSettingService = {
  getCsvColumnSettings: vi.fn(),
  updateCsvColumnSettings: vi.fn(),
  getNotificationChannelGroups: vi.fn(),
  updateNotificationChannelGroups: vi.fn(),
  getReminderSchedule: vi.fn(),
  updateReminderSchedule: vi.fn(),
};

const mockNotificationService = {
  showSuccess: vi.fn(),
  showError: vi.fn(),
};

const defaultCsvSettings = { nameColumn: 'Name', summeColumn: 'Summe' };
const defaultNotifSettings = {
  creation: { sendEmail: true, sendSlack: false, sendPush: true },
  confirmation: { sendEmail: true, sendSlack: false, sendPush: true },
  reminders: { sendEmail: true, sendSlack: false, sendPush: true },
  deletion: { sendEmail: false, sendSlack: false, sendPush: true },
  invoiceApproval: { sendEmail: true, sendSlack: false, sendPush: true },
  invoiceRejection: { sendEmail: true, sendSlack: false, sendPush: true },
  invoiceChangesRequested: { sendEmail: true, sendSlack: false, sendPush: true },
  invoicePaymentCompleted: { sendEmail: true, sendSlack: false, sendPush: true },
};
const defaultReminderSettings = {
  daysBeforeDue: [7, 2, 1],
  runAtHourUtc: 8,
  runAtMinuteUtc: 0,
  emailDelayMs: 500,
};

describe('AdminSettingsSettingsPageComponent', () => {
  let component: AdminSettingsSettingsPageComponent;
  let fixture: ComponentFixture<AdminSettingsSettingsPageComponent>;

  beforeEach(async () => {
    mockSystemSettingService.getCsvColumnSettings
      .mockReset()
      .mockReturnValue(of({ ...defaultCsvSettings }));
    mockSystemSettingService.updateCsvColumnSettings.mockReset().mockReturnValue(of(undefined));
    mockSystemSettingService.getNotificationChannelGroups.mockReset().mockReturnValue(
      of({
        creation: { ...defaultNotifSettings.creation },
        confirmation: { ...defaultNotifSettings.confirmation },
        reminders: { ...defaultNotifSettings.reminders },
        deletion: { ...defaultNotifSettings.deletion },
        invoiceApproval: { ...defaultNotifSettings.invoiceApproval },
        invoiceRejection: { ...defaultNotifSettings.invoiceRejection },
        invoiceChangesRequested: { ...defaultNotifSettings.invoiceChangesRequested },
        invoicePaymentCompleted: { ...defaultNotifSettings.invoicePaymentCompleted },
      }),
    );
    mockSystemSettingService.updateNotificationChannelGroups
      .mockReset()
      .mockReturnValue(of(undefined));
    mockSystemSettingService.getReminderSchedule
      .mockReset()
      .mockReturnValue(of({ ...defaultReminderSettings }));
    mockSystemSettingService.updateReminderSchedule.mockReset().mockReturnValue(of(undefined));
    mockNotificationService.showSuccess.mockReset();
    mockNotificationService.showError.mockReset();

    await TestBed.configureTestingModule({
      imports: [AdminSettingsSettingsPageComponent],
      providers: [
        { provide: SystemSettingService, useValue: mockSystemSettingService },
        { provide: NotificationService, useValue: mockNotificationService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AdminSettingsSettingsPageComponent);
    component = fixture.componentInstance;
    // detectChanges() is called explicitly in each test to control when ngOnInit runs
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  // ── Group A: ngOnInit + load methods ───────────────────────────────────────

  describe('ngOnInit', () => {
    it('should call loadCsvSettings, loadNotifSettings, and loadReminderSettings', () => {
      const spyCsv = vi.spyOn(component, 'loadCsvSettings');
      const spyNotif = vi.spyOn(component, 'loadNotifSettings');
      const spyReminder = vi.spyOn(component, 'loadReminderSettings');

      fixture.detectChanges();

      expect(spyCsv).toHaveBeenCalledOnce();
      expect(spyNotif).toHaveBeenCalledOnce();
      expect(spyReminder).toHaveBeenCalledOnce();
    });
  });

  describe('loadCsvSettings', () => {
    it('should set csvSettings and csvOriginal from the service response', () => {
      fixture.detectChanges();

      expect(component.csvSettings).toEqual(defaultCsvSettings);
      expect(component.csvLoading).toBe(false);
    });

    it('should set csvLoading to false and not crash on service error', () => {
      mockSystemSettingService.getCsvColumnSettings.mockReturnValue(
        throwError(() => new Error('load failed')),
      );

      fixture.detectChanges();

      expect(component.csvLoading).toBe(false);
    });
  });

  describe('loadNotifSettings', () => {
    it('should set notifSettings from the service response', () => {
      fixture.detectChanges();

      expect(component.notifSettings).toEqual(defaultNotifSettings);
      expect(component.notifLoading).toBe(false);
    });

    it('should set notifLoading to false on service error', () => {
      mockSystemSettingService.getNotificationChannelGroups.mockReturnValue(
        throwError(() => new Error('load failed')),
      );

      fixture.detectChanges();

      expect(component.notifLoading).toBe(false);
    });
  });

  describe('loadReminderSettings', () => {
    it('should populate reminderDaysInput, reminderTimeInput, and reminderEmailDelayInput', () => {
      fixture.detectChanges();

      expect(component.reminderDaysInput).toBe('7, 2, 1');
      expect(component.reminderTimeInput).toBe('08:00');
      expect(component.reminderEmailDelayInput).toBe(500);
      expect(component.reminderLoading).toBe(false);
    });

    it('should set reminderLoading to false on service error', () => {
      mockSystemSettingService.getReminderSchedule.mockReturnValue(
        throwError(() => new Error('load failed')),
      );

      fixture.detectChanges();

      expect(component.reminderLoading).toBe(false);
    });
  });

  // ── Group B: save methods ──────────────────────────────────────────────────

  describe('saveCsvSettings', () => {
    it('should update csvOriginal and show success notification on success', () => {
      fixture.detectChanges();
      component.csvSettings = { nameColumn: 'Bezeichnung', summeColumn: 'Betrag' };

      component.saveCsvSettings();

      expect(mockSystemSettingService.updateCsvColumnSettings).toHaveBeenCalledWith({
        nameColumn: 'Bezeichnung',
        summeColumn: 'Betrag',
      });
      expect(component.csvDirty).toBe(false);
      expect(component.csvSaving).toBe(false);
      expect(mockNotificationService.showSuccess).toHaveBeenCalledWith(
        'CSV column settings saved.',
      );
    });

    it('should show error notification and clear saving flag on failure', () => {
      fixture.detectChanges();
      mockSystemSettingService.updateCsvColumnSettings.mockReturnValue(
        throwError(() => new Error('Network error')),
      );

      component.saveCsvSettings();

      expect(component.csvSaving).toBe(false);
      expect(mockNotificationService.showError).toHaveBeenCalledWith('Network error');
    });
  });

  describe('saveNotifSettings', () => {
    it('should update notifOriginal and show success notification on success', () => {
      fixture.detectChanges();
      component.notifSettings = {
        creation: { sendEmail: false, sendSlack: true, sendPush: true },
        confirmation: { sendEmail: true, sendSlack: false, sendPush: true },
        reminders: { sendEmail: false, sendSlack: false, sendPush: false },
        deletion: { sendEmail: false, sendSlack: false, sendPush: false },
        invoiceApproval: { sendEmail: true, sendSlack: false, sendPush: true },
        invoiceRejection: { sendEmail: true, sendSlack: false, sendPush: true },
        invoiceChangesRequested: { sendEmail: true, sendSlack: false, sendPush: true },
        invoicePaymentCompleted: { sendEmail: true, sendSlack: false, sendPush: true },
      };

      component.saveNotifSettings();

      expect(component.notifDirty).toBe(false);
      expect(component.notifSaving).toBe(false);
      expect(mockNotificationService.showSuccess).toHaveBeenCalledWith(
        'Notification channel settings saved.',
      );
    });

    it('should show error notification on failure', () => {
      fixture.detectChanges();
      mockSystemSettingService.updateNotificationChannelGroups.mockReturnValue(
        throwError(() => new Error('Save failed')),
      );

      component.saveNotifSettings();

      expect(component.notifSaving).toBe(false);
      expect(mockNotificationService.showError).toHaveBeenCalledWith('Save failed');
    });
  });

  describe('saveReminderSettings', () => {
    it('should call updateReminderSchedule with parsed values and show success', () => {
      fixture.detectChanges();

      component.saveReminderSettings();

      expect(mockSystemSettingService.updateReminderSchedule).toHaveBeenCalledWith({
        daysBeforeDue: [7, 2, 1],
        runAtHourUtc: 8,
        runAtMinuteUtc: 0,
        emailDelayMs: 500,
      });
      expect(mockNotificationService.showSuccess).toHaveBeenCalledWith('Reminder schedule saved.');
    });

    it('should not call service and set error when reminderDaysInput is invalid', () => {
      fixture.detectChanges();
      component.reminderDaysInput = 'invalid, input';

      component.saveReminderSettings();

      expect(mockSystemSettingService.updateReminderSchedule).not.toHaveBeenCalled();
      expect(component.reminderDaysError).toBeTruthy();
    });

    it('should show error notification on service failure', () => {
      fixture.detectChanges();
      mockSystemSettingService.updateReminderSchedule.mockReturnValue(
        throwError(() => new Error('Reminder save failed')),
      );

      component.saveReminderSettings();

      expect(component.reminderSaving).toBe(false);
      expect(mockNotificationService.showError).toHaveBeenCalledWith('Reminder save failed');
    });
  });

  // ── Group C: dirty getters ─────────────────────────────────────────────────

  describe('csvDirty', () => {
    it('should be false when settings match the loaded original', () => {
      fixture.detectChanges();
      expect(component.csvDirty).toBe(false);
    });

    it('should be true after nameColumn is changed', () => {
      fixture.detectChanges();
      component.csvSettings = { ...component.csvSettings, nameColumn: 'Bezeichnung' };
      expect(component.csvDirty).toBe(true);
    });
  });

  describe('notifDirty', () => {
    it('should be false when settings match the loaded original', () => {
      fixture.detectChanges();
      expect(component.notifDirty).toBe(false);
    });

    it('should be true after a channel flag is toggled', () => {
      fixture.detectChanges();
      component.notifSettings = {
        ...component.notifSettings,
        invoicePaymentCompleted: {
          ...component.notifSettings.invoicePaymentCompleted,
          sendSlack: true,
        },
      };
      expect(component.notifDirty).toBe(true);
    });
  });

  describe('reminderDirty', () => {
    it('should be false when inputs match the loaded original', () => {
      fixture.detectChanges();
      expect(component.reminderDirty).toBe(false);
    });

    it('should be true after reminderDaysInput is changed', () => {
      fixture.detectChanges();
      component.reminderDaysInput = '1, 2, 3';
      expect(component.reminderDirty).toBe(true);
    });
  });

  // ── Group D: validation / blur handlers ───────────────────────────────────

  describe('onReminderDaysBlur', () => {
    it('should clear reminderDaysError for valid comma-separated integers', () => {
      component.reminderDaysInput = '7, 2, 1';
      component.onReminderDaysBlur();
      expect(component.reminderDaysError).toBe('');
    });

    it('should set reminderDaysError for non-numeric input', () => {
      component.reminderDaysInput = 'a, b, c';
      component.onReminderDaysBlur();
      expect(component.reminderDaysError).not.toBe('');
    });
  });

  describe('onReminderTimeBlur', () => {
    it('should clear reminderTimeError for a valid HH:MM string', () => {
      component.reminderTimeInput = '08:30';
      component.onReminderTimeBlur();
      expect(component.reminderTimeError).toBe('');
    });

    it('should set reminderTimeError for an invalid time string', () => {
      component.reminderTimeInput = 'not-a-time';
      component.onReminderTimeBlur();
      expect(component.reminderTimeError).not.toBe('');
    });
  });

  describe('onReminderEmailDelayBlur', () => {
    it('should clear reminderEmailDelayError for a valid non-negative integer', () => {
      component.reminderEmailDelayInput = 1000;
      component.onReminderEmailDelayBlur();
      expect(component.reminderEmailDelayError).toBe('');
    });

    it('should set reminderEmailDelayError for a negative value', () => {
      component.reminderEmailDelayInput = -1;
      component.onReminderEmailDelayBlur();
      expect(component.reminderEmailDelayError).not.toBe('');
    });
  });
});
