import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { HomeManagerApiService } from '../../core/api/home-manager-api.service';
import { LogEntryDto, PowerManagerDecisionResult } from '../../core/models/api.models';

@Injectable()
export class LogsFacade {
  private readonly api = inject(HomeManagerApiService);

  getLogs(): Observable<LogEntryDto[]> {
    return this.api.getLogs(200);
  }

  getDecisions(): Observable<PowerManagerDecisionResult[]> {
    return this.api.getLatestDecisions(50);
  }
}
