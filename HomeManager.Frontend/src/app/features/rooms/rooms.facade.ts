import { Injectable, inject } from '@angular/core';
import { BehaviorSubject, Observable, catchError, map, of, shareReplay, startWith, switchMap, tap } from 'rxjs';
import { HomeManagerApiService } from '../../core/api/home-manager-api.service';
import { RoomConfiguration } from '../../core/models/api.models';
import { RequestState } from '../../shared/models/request-state.model';

@Injectable()
export class RoomsFacade {
  private readonly api = inject(HomeManagerApiService);
  private readonly reloadSubject = new BehaviorSubject<void>(undefined);

  readonly state$: Observable<RequestState<RoomConfiguration[]>> = this.reloadSubject.pipe(
    switchMap(() =>
      this.api.getRooms().pipe(
        map((data) => ({ status: 'loaded', data } as RequestState<RoomConfiguration[]>)),
        startWith({ status: 'loading' } as RequestState<RoomConfiguration[]>),
        catchError(() => of({ status: 'error', errorMessage: 'Nie udało się pobrać pokoi.' } as RequestState<RoomConfiguration[]>))
      )
    ),
    shareReplay({ bufferSize: 1, refCount: true })
  );

  createRoom(room: RoomConfiguration): Observable<RoomConfiguration> {
    return this.api.createRoom(room).pipe(tap(() => this.refresh()));
  }

  deleteRoom(id: string): Observable<void> {
    return this.api.deleteRoom(id).pipe(tap(() => this.refresh()));
  }

  refresh(): void {
    this.reloadSubject.next();
  }
}
