import { AsyncPipe, NgFor, NgIf } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { HaEntitiesFacade } from './ha-entities.facade';

@Component({
  selector: 'app-ha-entities-page',
  standalone: true,
  imports: [AsyncPipe, NgFor, NgIf],
  providers: [HaEntitiesFacade],
  template: `
    <h2>HA Entities</h2>
    <button type="button" (click)="refreshFromHomeAssistant()">Odśwież z Home Assistant</button>

    <ng-container *ngIf="facade.state$ | async as state">
      <p *ngIf="state.status === 'loading'">Ładowanie encji...</p>
      <p *ngIf="state.status === 'error'">{{ state.errorMessage }}</p>
      <p *ngIf="state.status === 'loaded' && state.data?.length === 0">Brak encji.</p>

      <table *ngIf="state.status === 'loaded' && state.data && state.data.length > 0">
        <thead>
          <tr>
            <th>Entity ID</th>
            <th>Domain</th>
            <th>State</th>
            <th>Friendly name</th>
          </tr>
        </thead>
        <tbody>
          <tr *ngFor="let entity of state.data">
            <td>{{ entity.entityId }}</td>
            <td>{{ entity.domain }}</td>
            <td>{{ entity.state }}</td>
            <td>{{ entity.friendlyName }}</td>
          </tr>
        </tbody>
      </table>
    </ng-container>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class HaEntitiesPageComponent {
  protected readonly facade = inject(HaEntitiesFacade);

  protected refreshFromHomeAssistant(): void {
    this.facade.refresh().subscribe({
      next: () => this.facade.reload()
    });
  }
}
