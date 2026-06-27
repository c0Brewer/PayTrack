//AI helped with the test cases

import { TestBed } from '@angular/core/testing';
import { firstValueFrom } from 'rxjs';
import { vi } from 'vitest';

import { client } from '../../client';
import {
  CsvColumnSettingsDto,
  InvoiceSubmissionSettingsDto,
  NotificationChannelGroupsDto,
  ReminderScheduleDto,
} from '../../types/exporter';

import { SystemSettingService } from './system-setting-service';

describe('SystemSettingService', () => {
  let service: SystemSettingService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(SystemSettingService);
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('getCsvColumnSettings', () => {
    it('should return CSV column settings on success', async () => {
      const response: CsvColumnSettingsDto = { nameColumn: 'Name', summeColumn: 'Summe' };
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      vi.spyOn(client, 'GET').mockResolvedValue({ data: response, error: null } as any);

      const result = await firstValueFrom(service.getCsvColumnSettings());

      expect(result).toEqual(response);
    });

    it('should throw with the error detail when API returns error', async () => {
      vi.spyOn(client, 'GET').mockResolvedValue({
        data: null,
        error: { detail: 'Failed to load CSV column settings' },
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any);

      await expect(firstValueFrom(service.getCsvColumnSettings())).rejects.toThrow(
        'Failed to load CSV column settings',
      );
    });
  });

  describe('updateCsvColumnSettings', () => {
    it('should resolve without a value on success', async () => {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      vi.spyOn(client, 'PUT').mockResolvedValue({ error: null } as any);

      await expect(
        firstValueFrom(
          service.updateCsvColumnSettings({ nameColumn: 'Bezeichnung', summeColumn: 'Betrag' }),
        ),
      ).resolves.toBeUndefined();
    });

    it('should throw with fallback message when API error has no detail', async () => {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      vi.spyOn(client, 'PUT').mockResolvedValue({ error: {} } as any);

      await expect(
        firstValueFrom(service.updateCsvColumnSettings({ nameColumn: 'X', summeColumn: 'Y' })),
      ).rejects.toThrow('Failed to update CSV column settings');
    });
  });

  describe('getInvoiceSubmissionSettings', () => {
    it('should return invoice submission settings on success', async () => {
      const response: InvoiceSubmissionSettingsDto = { receiptExtractionEnabled: false };
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      vi.spyOn(client, 'GET').mockResolvedValue({ data: response, error: null } as any);

      const result = await firstValueFrom(service.getInvoiceSubmissionSettings());

      expect(result).toEqual(response);
    });

    it('should throw when API returns error', async () => {
      vi.spyOn(client, 'GET').mockResolvedValue({
        data: null,
        error: { detail: 'Failed to load invoice submission settings' },
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any);

      await expect(firstValueFrom(service.getInvoiceSubmissionSettings())).rejects.toThrow(
        'Failed to load invoice submission settings',
      );
    });
  });

  describe('getNotificationChannelGroups', () => {
    it('should return notification channel groups on success', async () => {
      const response: NotificationChannelGroupsDto = {
        creation: { sendEmail: true, sendSlack: false, sendPush: true },
        confirmation: { sendEmail: false, sendSlack: true, sendPush: true },
        reminders: { sendEmail: true, sendSlack: true, sendPush: false },
        deletion: { sendEmail: false, sendSlack: false, sendPush: false },
        invoiceApproval: { sendEmail: true, sendSlack: false, sendPush: true },
        invoiceRejection: { sendEmail: true, sendSlack: false, sendPush: true },
        invoiceChangesRequested: { sendEmail: true, sendSlack: false, sendPush: true },
        invoicePaymentCompleted: { sendEmail: true, sendSlack: false, sendPush: true },
      };
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      vi.spyOn(client, 'GET').mockResolvedValue({ data: response, error: null } as any);

      const result = await firstValueFrom(service.getNotificationChannelGroups());

      expect(result).toEqual(response);
    });

    it('should throw when API returns error', async () => {
      vi.spyOn(client, 'GET').mockResolvedValue({
        data: null,
        error: { detail: 'Failed to load notification channels' },
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any);

      await expect(firstValueFrom(service.getNotificationChannelGroups())).rejects.toThrow(
        'Failed to load notification channels',
      );
    });
  });

  describe('updateNotificationChannelGroups', () => {
    it('should resolve without a value on success', async () => {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      vi.spyOn(client, 'PUT').mockResolvedValue({ error: null } as any);

      await expect(
        firstValueFrom(
          service.updateNotificationChannelGroups({
            creation: { sendEmail: true, sendSlack: false, sendPush: true },
            confirmation: { sendEmail: true, sendSlack: false, sendPush: true },
            reminders: { sendEmail: true, sendSlack: false, sendPush: true },
            deletion: { sendEmail: false, sendSlack: false, sendPush: false },
            invoiceApproval: { sendEmail: true, sendSlack: false, sendPush: true },
            invoiceRejection: { sendEmail: true, sendSlack: false, sendPush: true },
            invoiceChangesRequested: { sendEmail: true, sendSlack: false, sendPush: true },
            invoicePaymentCompleted: { sendEmail: true, sendSlack: false, sendPush: true },
          }),
        ),
      ).resolves.toBeUndefined();
    });

    it('should throw when API returns error', async () => {
      vi.spyOn(client, 'PUT').mockResolvedValue({
        error: { detail: 'Failed to update notification channels' },
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any);

      await expect(
        firstValueFrom(
          service.updateNotificationChannelGroups({
            creation: { sendEmail: true, sendSlack: false, sendPush: true },
            confirmation: { sendEmail: true, sendSlack: false, sendPush: true },
            reminders: { sendEmail: true, sendSlack: false, sendPush: true },
            deletion: { sendEmail: false, sendSlack: false, sendPush: false },
            invoiceApproval: { sendEmail: true, sendSlack: false, sendPush: true },
            invoiceRejection: { sendEmail: true, sendSlack: false, sendPush: true },
            invoiceChangesRequested: { sendEmail: true, sendSlack: false, sendPush: true },
            invoicePaymentCompleted: { sendEmail: true, sendSlack: false, sendPush: true },
          }),
        ),
      ).rejects.toThrow('Failed to update notification channels');
    });
  });

  describe('getReminderSchedule', () => {
    it('should return reminder schedule on success', async () => {
      const response: ReminderScheduleDto = {
        daysBeforeDue: [7, 2, 1],
        runAtHourUtc: 8,
        runAtMinuteUtc: 0,
        emailDelayMs: 500,
      };
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      vi.spyOn(client, 'GET').mockResolvedValue({ data: response, error: null } as any);

      const result = await firstValueFrom(service.getReminderSchedule());

      expect(result).toEqual(response);
    });

    it('should throw when API returns error', async () => {
      vi.spyOn(client, 'GET').mockResolvedValue({
        data: null,
        error: { detail: 'Failed to load reminder schedule' },
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any);

      await expect(firstValueFrom(service.getReminderSchedule())).rejects.toThrow(
        'Failed to load reminder schedule',
      );
    });
  });

  describe('updateReminderSchedule', () => {
    it('should resolve without a value on success', async () => {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      vi.spyOn(client, 'PUT').mockResolvedValue({ error: null } as any);

      await expect(
        firstValueFrom(
          service.updateReminderSchedule({
            daysBeforeDue: [7, 2, 1],
            runAtHourUtc: 8,
            runAtMinuteUtc: 0,
            emailDelayMs: 500,
          }),
        ),
      ).resolves.toBeUndefined();
    });

    it('should throw when API returns error', async () => {
      vi.spyOn(client, 'PUT').mockResolvedValue({
        error: { detail: 'Failed to update reminder schedule' },
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any);

      await expect(
        firstValueFrom(
          service.updateReminderSchedule({
            daysBeforeDue: [7],
            runAtHourUtc: 8,
            runAtMinuteUtc: 0,
            emailDelayMs: 500,
          }),
        ),
      ).rejects.toThrow('Failed to update reminder schedule');
    });
  });
});
