import { Injectable, inject } from '@angular/core';
import { BehaviorSubject, Observable, catchError, map, of, shareReplay, startWith, switchMap } from 'rxjs';
import { HomeManagerApiService } from '../../core/api/home-manager-api.service';
import { HomeAssistantEntityDto } from '../../core/models/api.models';
import { RequestState } from '../../shared/models/request-state.model';

@Injectable()
export class HaEntitiesFacade {
  private readonly api = inject(HomeManagerApiService);
  private readonly reloadSubject = new BehaviorSubject<void>(undefined);

  readonly state$: Observable<RequestState<HomeAssistantEntityDto[]>> = this.reloadSubject.pipe(
    switchMap(() =>
      this.api.getHaEntities().pipe(
        map((data) => ({ status: 'loaded', data } as RequestState<HomeAssistantEntityDto[]>)),
        startWith({ status: 'loading' } as RequestState<HomeAssistantEntityDto[]>),
        catchError(() => of({ status: 'error', errorMessage: 'Nie udało się pobrać encji HA.' } as RequestState<HomeAssistantEntityDto[]>))
      )
    ),
    shareReplay({ bufferSize: 1, refCount: true })
  );

  refresh(): Observable<{ refreshed: number }> {
    return this.api.refreshHaEntities();
  }

  reload(): void {
    this.reloadSubject.next();
  }
}
