import { Injectable, inject } from '@angular/core';
import { BehaviorSubject, Observable, catchError, map, of, shareReplay, startWith, switchMap, tap } from 'rxjs';
import { HomeManagerApiService } from '../../core/api/home-manager-api.service';
import { ManualOverrideDefinition } from '../../core/models/api.models';
import { RequestState } from '../../shared/models/request-state.model';

@Injectable()
export class OverridesFacade {
  private readonly api = inject(HomeManagerApiService);
  private readonly reloadSubject = new BehaviorSubject<void>(undefined);

  readonly state$: Observable<RequestState<ManualOverrideDefinition[]>> = this.reloadSubject.pipe(
    switchMap(() =>
      this.api.getOverrides().pipe(
        map((data) => ({ status: 'loaded', data } as RequestState<ManualOverrideDefinition[]>)),
        startWith({ status: 'loading' } as RequestState<ManualOverrideDefinition[]>),
        catchError(() => of({ status: 'error', errorMessage: 'Nie udało się pobrać override.' } as RequestState<ManualOverrideDefinition[]>))
      )
    ),
    shareReplay({ bufferSize: 1, refCount: true })
  );

  createOverride(entry: ManualOverrideDefinition): Observable<ManualOverrideDefinition> {
    return this.api.createOverride(entry).pipe(tap(() => this.refresh()));
  }

  deleteOverride(id: string): Observable<void> {
    return this.api.deleteOverride(id).pipe(tap(() => this.refresh()));
  }

  refresh(): void {
    this.reloadSubject.next();
  }
}
