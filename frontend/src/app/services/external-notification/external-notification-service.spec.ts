import { TestBed } from '@angular/core/testing';
import { firstValueFrom } from 'rxjs';

import { client } from '../../client';

import { ExternalNotificationService } from './external-notification-service';

describe('ExternalNotificationService', () => {
  let service: ExternalNotificationService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ExternalNotificationService);
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('sendEmail', () => {
    it('should call POST /api/v1/notification/email with the correct body', async () => {
      vi.spyOn(client, 'POST').mockResolvedValue({
        error: null,
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any);

      await firstValueFrom(service.sendEmail('user@example.com', 'Test Subject', 'Test Body'));

      expect(client.POST).toHaveBeenCalledWith('/api/v1/notification/email', {
        body: { recipientEmail: 'user@example.com', subject: 'Test Subject', body: 'Test Body' },
      });
    });

    it('should resolve without a value on success', async () => {
      vi.spyOn(client, 'POST').mockResolvedValue({
        error: null,
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any);

      await expect(
        firstValueFrom(service.sendEmail('user@example.com', 'Subject', 'Body')),
      ).resolves.toBeUndefined();
    });

    it('should throw the backend error detail when the request fails', async () => {
      vi.spyOn(client, 'POST').mockResolvedValue({
        error: { detail: 'Email delivery failed' },
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any);

      await expect(
        firstValueFrom(service.sendEmail('user@example.com', 'Subject', 'Body')),
      ).rejects.toThrow('Email delivery failed');
    });

    it('should throw the default error message when no detail is provided', async () => {
      vi.spyOn(client, 'POST').mockResolvedValue({
        error: {},
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any);

      await expect(
        firstValueFrom(service.sendEmail('user@example.com', 'Subject', 'Body')),
      ).rejects.toThrow('Unexpected Error');
    });
  });

  describe('sendSlack', () => {
    it('should call POST /api/v1/notification/slack with the correct body', async () => {
      vi.spyOn(client, 'POST').mockResolvedValue({
        error: null,
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any);

      await firstValueFrom(service.sendSlack('user@example.com', 'Test Message'));

      expect(client.POST).toHaveBeenCalledWith('/api/v1/notification/slack', {
        body: { recipientEmail: 'user@example.com', message: 'Test Message' },
      });
    });

    it('should resolve without a value on success', async () => {
      vi.spyOn(client, 'POST').mockResolvedValue({
        error: null,
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any);

      await expect(
        firstValueFrom(service.sendSlack('user@example.com', 'Message')),
      ).resolves.toBeUndefined();
    });

    it('should throw the backend error detail when the request fails', async () => {
      vi.spyOn(client, 'POST').mockResolvedValue({
        error: { detail: 'Slack user not found' },
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any);

      await expect(
        firstValueFrom(service.sendSlack('user@example.com', 'Message')),
      ).rejects.toThrow('Slack user not found');
    });

    it('should throw the default error message when no detail is provided', async () => {
      vi.spyOn(client, 'POST').mockResolvedValue({
        error: {},
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any);

      await expect(
        firstValueFrom(service.sendSlack('user@example.com', 'Message')),
      ).rejects.toThrow('Unexpected Error');
    });
  });
});
