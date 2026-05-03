import { Injectable, inject } from '@angular/core';
import { BehaviorSubject, Observable, catchError, map, of, shareReplay, startWith, switchMap } from 'rxjs';
import { HomeManagerApiService } from '../../core/api/home-manager-api.service';
import { PowerManagerDecisionResult, RoomConfiguration } from '../../core/models/api.models';
import { RequestState } from '../../shared/models/request-state.model';

export interface EnergyManagerVm {
  rooms: RoomConfiguration[];
  latestDecision?: PowerManagerDecisionResult;
}

@Injectable()
export class EnergyManagerFacade {
  private readonly api = inject(HomeManagerApiService);
  private readonly reloadSubject = new BehaviorSubject<void>(undefined);

  readonly state$: Observable<RequestState<EnergyManagerVm>> = this.reloadSubject.pipe(
    switchMap(() =>
      this.api.getRooms().pipe(
        switchMap((rooms) =>
          this.api.getLatestDecisions(1).pipe(
            map((decisions) => ({
              status: 'loaded',
              data: {
                rooms,
                latestDecision: decisions[0]
              }
            } as RequestState<EnergyManagerVm>))
          )
        ),
        startWith({ status: 'loading' } as RequestState<EnergyManagerVm>),
        catchError(() => of({ status: 'error', errorMessage: 'Nie udało się pobrać danych Energy Manager.' } as RequestState<EnergyManagerVm>))
      )
    ),
    shareReplay({ bufferSize: 1, refCount: true })
  );

  evaluateNow(): Observable<PowerManagerDecisionResult> {
    return this.api.evaluatePowerManager(false);
  }

  refresh(): void {
    this.reloadSubject.next();
  }
}
