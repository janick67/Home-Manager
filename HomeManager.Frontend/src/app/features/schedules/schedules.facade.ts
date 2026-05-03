import { Injectable, inject } from '@angular/core';
import { BehaviorSubject, Observable, catchError, map, of, shareReplay, startWith, switchMap, tap } from 'rxjs';
import { HomeManagerApiService } from '../../core/api/home-manager-api.service';
import { ScheduleDefinition } from '../../core/models/api.models';
import { RequestState } from '../../shared/models/request-state.model';

@Injectable()
export class SchedulesFacade {
  private readonly api = inject(HomeManagerApiService);
  private readonly reloadSubject = new BehaviorSubject<void>(undefined);

  readonly state$: Observable<RequestState<ScheduleDefinition[]>> = this.reloadSubject.pipe(
    switchMap(() =>
      this.api.getSchedules().pipe(
        map((data) => ({ status: 'loaded', data } as RequestState<ScheduleDefinition[]>)),
        startWith({ status: 'loading' } as RequestState<ScheduleDefinition[]>),
        catchError(() => of({ status: 'error', errorMessage: 'Nie udało się pobrać harmonogramów.' } as RequestState<ScheduleDefinition[]>))
      )
    ),
    shareReplay({ bufferSize: 1, refCount: true })
  );

  createSchedule(schedule: ScheduleDefinition): Observable<ScheduleDefinition> {
    return this.api.createSchedule(schedule).pipe(tap(() => this.refresh()));
  }

  deleteSchedule(id: string): Observable<void> {
    return this.api.deleteSchedule(id).pipe(tap(() => this.refresh()));
  }

  refresh(): void {
    this.reloadSubject.next();
  }
}
