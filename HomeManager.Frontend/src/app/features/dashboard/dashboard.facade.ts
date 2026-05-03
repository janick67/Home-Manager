import { Injectable, inject } from '@angular/core';
import { BehaviorSubject, Observable, catchError, map, of, shareReplay, startWith, switchMap } from 'rxjs';
import { HomeManagerApiService } from '../../core/api/home-manager-api.service';
import { DashboardDto } from '../../core/models/api.models';
import { RequestState } from '../../shared/models/request-state.model';

@Injectable()
export class DashboardFacade {
  private readonly api = inject(HomeManagerApiService);
  private readonly reloadSubject = new BehaviorSubject<void>(undefined);

  readonly state$: Observable<RequestState<DashboardDto>> = this.reloadSubject.pipe(
    switchMap(() =>
      this.api.getDashboard().pipe(
        map((data) => ({ status: 'loaded', data } as RequestState<DashboardDto>)),
        startWith({ status: 'loading' } as RequestState<DashboardDto>),
        catchError(() => of({ status: 'error', errorMessage: 'Nie udało się pobrać dashboardu.' } as RequestState<DashboardDto>))
      )
    ),
    shareReplay({ bufferSize: 1, refCount: true })
  );

  refresh(): void {
    this.reloadSubject.next();
  }
}
