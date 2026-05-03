import { AsyncPipe, NgFor, NgIf } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { DashboardFacade } from './dashboard.facade';

@Component({
  selector: 'app-dashboard-page',
  standalone: true,
  imports: [AsyncPipe, NgFor, NgIf],
  providers: [DashboardFacade],
  template: `
    <h2>Dashboard</h2>
    <ng-container *ngIf="facade.state$ | async as state">
      <p *ngIf="state.status === 'loading'">Ładowanie danych dashboardu...</p>
      <p *ngIf="state.status === 'error'">{{ state.errorMessage }}</p>

      <section *ngIf="state.status === 'loaded' && state.data as data">
        <p>Połączenie z HA: <strong>{{ data.homeAssistantConnected ? 'OK' : 'Brak' }}</strong></p>
        <p>EnergyMode: <strong>{{ data.energyMode }}</strong> | PresenceMode: <strong>{{ data.presenceMode }}</strong></p>
        <p>PV: {{ data.pvProductionWatts }} W | Grid import: {{ data.gridImportWatts }} W | Grid export: {{ data.gridExportWatts }} W</p>
        <p>Bateria: SOC {{ data.batterySocPercent }}% | charge {{ data.batteryChargeWatts }} W | discharge {{ data.batteryDischargeWatts }} W</p>
        <p>Dostępna moc: <strong>{{ data.availablePowerWatts }} W</strong></p>
        <p>Aktywne harmonogramy: {{ data.activeSchedules }} | Aktywne override: {{ data.activeOverrides }}</p>

        <h3>Ostatnie decyzje</h3>
        <p *ngIf="data.latestDecisions.length === 0">Brak decyzji.</p>
        <ul *ngIf="data.latestDecisions.length > 0">
          <li *ngFor="let decision of data.latestDecisions">
            {{ decision.evaluatedAtUtc }} — {{ decision.effectiveEnergyMode }} / {{ decision.effectivePresenceMode }}
          </li>
        </ul>
      </section>
    </ng-container>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DashboardPageComponent {
  protected readonly facade = inject(DashboardFacade);
}
