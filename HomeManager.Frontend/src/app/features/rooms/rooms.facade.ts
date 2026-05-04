import { Injectable, inject } from '@angular/core';
import { BehaviorSubject, Observable, catchError, forkJoin, map, of, shareReplay, startWith, switchMap, tap } from 'rxjs';
import { HomeManagerApiService } from '../../core/api/home-manager-api.service';
import { HomeAssistantEntityDto, RoomConfiguration } from '../../core/models/api.models';
import { RequestState } from '../../shared/models/request-state.model';

export interface RoomsVm {
  rooms: RoomConfiguration[];
  climateEntities: HomeAssistantEntityDto[];
}

@Injectable()
export class RoomsFacade {
  private readonly api = inject(HomeManagerApiService);
  private readonly reloadSubject = new BehaviorSubject<void>(undefined);

  readonly state$: Observable<RequestState<RoomsVm>> = this.reloadSubject.pipe(
    switchMap(() =>
      forkJoin({
        rooms: this.api.getRooms(),
        entities: this.api.getHaEntities()
      }).pipe(
        map(({ rooms, entities }) => ({
          status: 'loaded',
          data: {
            rooms,
            climateEntities: entities
              .filter((entity) => entity.domain === 'climate')
              .sort((left, right) => (left.friendlyName ?? left.entityId).localeCompare(right.friendlyName ?? right.entityId))
          }
        } as RequestState<RoomsVm>)),
        startWith({ status: 'loading' } as RequestState<RoomsVm>),
        catchError(() => of({ status: 'error', errorMessage: 'Nie udało się pobrać pokoi lub encji HA.' } as RequestState<RoomsVm>))
      )
    ),
    shareReplay({ bufferSize: 1, refCount: true })
  );

  createRoom(room: RoomConfiguration): Observable<RoomConfiguration> {
    return this.api.createRoom(room).pipe(tap(() => this.refresh()));
  }
  updateRoom(room: RoomConfiguration): Observable<void> {
    return this.api.updateRoom(room).pipe(tap(() => this.refresh()));
  }

  deleteRoom(id: string): Observable<void> {
    return this.api.deleteRoom(id).pipe(tap(() => this.refresh()));
  }

  refreshClimateEntities(): Observable<{ refreshed: number }> {
    return this.api.refreshHaEntities().pipe(tap(() => this.refresh()));
  }

  refresh(): void {
    this.reloadSubject.next();
  }
}
