import { Injectable } from '@angular/core';
import {from, Observable} from 'rxjs';
import {UserSettingsDto} from '../../types/exporter';
import {client} from '../../client';

@Injectable({
  providedIn: 'root',
})
export class UserService {
  public getUserSettings(): Observable<UserSettingsDto> {
    const promise = client
      .GET('/api/v1/usersettings', {
        params: {},
      })
      .then(({ data, error }) => {
        if (error) throw new Error(error.detail ?? 'Unexpected Error');
        return data as UserSettingsDto;
      });

    return from(promise);
  }

  public updateUserSettings(settings: UserSettingsDto): Observable<void> {
    const promise = client
      .PUT('/api/v1/usersettings', {
        params: {},
        body: settings
      })
      .then(({ data, error }) => {
        if (error) throw new Error(error.detail ?? 'Unexpected Error');
        return data;
      });

    return from(promise);
  }
}
