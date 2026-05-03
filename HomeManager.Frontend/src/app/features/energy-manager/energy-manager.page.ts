import { AsyncPipe, NgFor, NgIf } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { EnergyManagerFacade } from './energy-manager.facade';

@Component({
  selector: 'app-energy-manager-page',
  standalone: true,
  imports: [AsyncPipe, NgFor, NgIf],
  providers: [EnergyManagerFacade],
  template: `
    <h2>Energy / Power Manager</h2>
    <button type="button" (click)="runEvaluation()">Uruchom evaluate (bez wysyłki komend)</button>

    <ng-container *ngIf="facade.state$ | async as state">
      <p *ngIf="state.status === 'loading'">Ładowanie...</p>
      <p *ngIf="state.status === 'error'">{{ state.errorMessage }}</p>

      <table *ngIf="state.status === 'loaded' && state.data">
        <thead>
          <tr>
            <th>Pokój</th>
            <th>Moc</th>
            <th>Priorytet</th>
            <th>Storage</th>
            <th>PV boost</th>
          </tr>
        </thead>
        <tbody>
          <tr *ngFor="let room of state.data.rooms">
            <td>{{ room.name }}</td>
            <td>{{ room.powerWatts }} W</td>
            <td>{{ room.priority }}</td>
            <td>{{ room.canStoreHeat ? 'Tak' : 'Nie' }}</td>
            <td>{{ room.allowPvBoost ? 'Tak' : 'Nie' }}</td>
          </tr>
        </tbody>
      </table>

      <p *ngIf="state.status === 'loaded' && state.data?.latestDecision as decision">
        Ostatnia decyzja: {{ decision.evaluatedAtUtc }} ({{ decision.effectiveEnergyMode }} / {{ decision.effectivePresenceMode }})
      </p>
    </ng-container>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EnergyManagerPageComponent {
  protected readonly facade = inject(EnergyManagerFacade);

  protected runEvaluation(): void {
    this.facade.evaluateNow().subscribe({
      next: () => this.facade.refresh()
    });
  }
}
