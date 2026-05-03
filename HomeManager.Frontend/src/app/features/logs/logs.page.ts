import { AsyncPipe, NgFor, NgIf } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { combineLatest, map } from 'rxjs';
import { LogsFacade } from './logs.facade';

@Component({
  selector: 'app-logs-page',
  standalone: true,
  imports: [AsyncPipe, NgFor, NgIf],
  providers: [LogsFacade],
  template: `
    <h2>Logs / Decision History</h2>
    <ng-container *ngIf="vm$ | async as vm">
      <h3>Logi systemowe</h3>
      <p *ngIf="vm.logs.length === 0">Brak logów.</p>
      <ul>
        <li *ngFor="let log of vm.logs">
          {{ log.timestampUtc }} [{{ log.level }}] {{ log.message }}
        </li>
      </ul>

      <h3>Ostatnie decyzje</h3>
      <p *ngIf="vm.decisions.length === 0">Brak decyzji.</p>
      <ul>
        <li *ngFor="let decision of vm.decisions">
          {{ decision.evaluatedAtUtc }} | {{ decision.effectiveEnergyMode }} | {{ decision.effectivePresenceMode }}
        </li>
      </ul>
    </ng-container>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LogsPageComponent {
  private readonly facade = inject(LogsFacade);

  protected readonly vm$ = combineLatest([
    this.facade.getLogs(),
    this.facade.getDecisions()
  ]).pipe(
    map(([logs, decisions]) => ({ logs, decisions }))
  );
}
