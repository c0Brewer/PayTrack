import { Injectable } from '@angular/core';
import { from, Observable } from 'rxjs';

import { client } from '../../client';
import {
  GetUserOptions,
  UpdateUserDto,
  UserDto,
  UserDtoPaginatedResponse,
} from '../../types/exporter';

@Injectable({
  providedIn: 'root',
})
export class UserService {
  public getUser(queryOptions: GetUserOptions): Observable<UserDtoPaginatedResponse> {
    const promise = client
      .GET('/api/v1/user', {
        params: {
          query: queryOptions,
        },
      })
      .then(({ data, error }) => {
        if (error) throw new Error(error.detail ?? 'Unexpected Error');
        return data;
      });

    return from(promise);
  }

  public updateUser(userId: number, updateRequest: UpdateUserDto): Observable<UserDto> {
    const promise = client
      .PUT('/api/v1/user/{id}', {
        params: {
          path: {
            id: userId,
          },
        },
        body: updateRequest,
      })
      .then(({ data, error }) => {
        if (error) throw new Error(error.detail ?? 'Unexpected Error');
        return data;
      });

    return from(promise);
  }
}
