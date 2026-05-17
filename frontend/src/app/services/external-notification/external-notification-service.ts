import { Injectable } from '@angular/core';
import { from, Observable } from 'rxjs';

import { client } from '../../client';

@Injectable({
  providedIn: 'root',
})
export class ExternalNotificationService {
  public sendEmail(recipientEmail: string, subject: string, body: string): Observable<void> {
    const promise = client
      .POST('/api/v1/notify/email', {
        body: { recipientEmail, subject, body },
      })
      .then(({ error }) => {
        if (error) throw new Error(error.detail ?? 'Unexpected Error');
      });

    return from(promise);
  }

  public sendSlack(recipientEmail: string, message: string): Observable<void> {
    const promise = client
      .POST('/api/v1/notify/slack', {
        body: { recipientEmail, message },
      })
      .then(({ error }) => {
        if (error) throw new Error(error.detail ?? 'Unexpected Error');
      });

    return from(promise);
  }
}
