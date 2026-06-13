import { Injectable } from '@angular/core';
import { from, Observable } from 'rxjs';

import { client } from '../../client';
import type {
  CsvColumnSettingsDto,
  NotificationChannelGroupsDto,
  ReminderScheduleDto,
  UpdateCsvColumnSettingsRequestDto,
  UpdateNotificationChannelGroupsRequestDto,
  UpdateReminderScheduleRequestDto,
} from '../../types/exporter';

type RawClient = {
  GET: (url: string) => Promise<{ data: unknown; error?: { detail?: string } }>;
  PUT: (url: string, init: { body: unknown }) => Promise<{ error?: { detail?: string } }>;
};

const BASE = '/api/v1/admin/settings';

@Injectable({ providedIn: 'root' })
export class SystemSettingService {
  private readonly raw = client as unknown as RawClient;

  getCsvColumnSettings(): Observable<CsvColumnSettingsDto> {
    return from(
      this.raw.GET(`${BASE}/csv-columns`).then(({ data, error }) => {
        if (error) throw new Error(error.detail ?? 'Failed to load CSV column settings');
        return data as CsvColumnSettingsDto;
      }),
    );
  }

  updateCsvColumnSettings(dto: UpdateCsvColumnSettingsRequestDto): Observable<void> {
    return from(
      this.raw.PUT(`${BASE}/csv-columns`, { body: dto }).then(({ error }) => {
        if (error) throw new Error(error.detail ?? 'Failed to update CSV column settings');
      }),
    );
  }

  getNotificationChannelGroups(): Observable<NotificationChannelGroupsDto> {
    return from(
      this.raw.GET(`${BASE}/notification-channels`).then(({ data, error }) => {
        if (error) throw new Error(error.detail ?? 'Failed to load notification channels');
        return data as NotificationChannelGroupsDto;
      }),
    );
  }

  updateNotificationChannelGroups(dto: UpdateNotificationChannelGroupsRequestDto): Observable<void> {
    return from(
      this.raw.PUT(`${BASE}/notification-channels`, { body: dto }).then(({ error }) => {
        if (error) throw new Error(error.detail ?? 'Failed to update notification channels');
      }),
    );
  }

  getReminderSchedule(): Observable<ReminderScheduleDto> {
    return from(
      this.raw.GET(`${BASE}/reminder-schedule`).then(({ data, error }) => {
        if (error) throw new Error(error.detail ?? 'Failed to load reminder schedule');
        return data as ReminderScheduleDto;
      }),
    );
  }

  updateReminderSchedule(dto: UpdateReminderScheduleRequestDto): Observable<void> {
    return from(
      this.raw.PUT(`${BASE}/reminder-schedule`, { body: dto }).then(({ error }) => {
        if (error) throw new Error(error.detail ?? 'Failed to update reminder schedule');
      }),
    );
  }
}
