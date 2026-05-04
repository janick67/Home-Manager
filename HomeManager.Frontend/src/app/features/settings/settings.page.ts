import { AsyncPipe, NgFor, NgIf } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { BehaviorSubject, catchError, combineLatest, map, of, shareReplay, startWith, switchMap, tap } from 'rxjs';
import { HomeAssistantEntityDto, SettingsDto } from '../../core/models/api.models';
import { SettingsFacade } from './settings.facade';

@Component({
  selector: 'app-settings-page',
  standalone: true,
  imports: [AsyncPipe, NgFor, NgIf, ReactiveFormsModule],
  providers: [SettingsFacade],
  template: `
    <h2>Settings</h2>

    <ng-container *ngIf="settings$ | async">
      <form [formGroup]="form" (ngSubmit)="save()">
        <h3>Połączenie i reguły</h3>
        <label>HA Base URL <input formControlName="homeAssistantBaseUrl"></label>
        <label>HA Token <input formControlName="homeAssistantToken"></label>
        <label>Safety margin watts <input type="number" formControlName="safetyMarginWatts"></label>
        <label>Max grid import watts <input type="number" formControlName="maxGridImportWatts"></label>
        <label>Max battery discharge watts <input type="number" formControlName="maxBatteryDischargeWatts"></label>
        <label>Min battery SOC for storage <input type="number" formControlName="minBatterySocForStoragePercent"></label>
        <label>Evaluation interval <input type="number" formControlName="evaluationIntervalSeconds"></label>
        <label>Command cooldown <input type="number" formControlName="commandCooldownSeconds"></label>
        <label>Heartbeat interval <input type="number" formControlName="heartbeatIntervalSeconds"></label>

        <h3>Mapowanie encji HA</h3>
        <div class="entity-picker">
          <label>Filtr encji HA
            <input [formControl]="entityFilterControl" placeholder="np. solar, grid, battery, sensor...">
          </label>
          <button type="button" (click)="refreshEntitiesFromHa()">Odśwież encje HA</button>
        </div>

        <ng-container *ngIf="filteredEntities$ | async as filteredEntities">
          <small>Dopasowanych encji: {{ filteredEntities.length }}</small>

          <label>PV production entity
            <select formControlName="pvProductionEntityId">
              <option value="">-- brak --</option>
              <option *ngFor="let entity of filteredEntities" [value]="entity.entityId">
                {{ entity.friendlyName || entity.entityId }} ({{ entity.entityId }})
              </option>
            </select>
          </label>

          <label>Grid import entity
            <select formControlName="gridImportEntityId">
              <option value="">-- brak --</option>
              <option *ngFor="let entity of filteredEntities" [value]="entity.entityId">
                {{ entity.friendlyName || entity.entityId }} ({{ entity.entityId }})
              </option>
            </select>
          </label>

          <label>Grid export entity
            <select formControlName="gridExportEntityId">
              <option value="">-- brak --</option>
              <option *ngFor="let entity of filteredEntities" [value]="entity.entityId">
                {{ entity.friendlyName || entity.entityId }} ({{ entity.entityId }})
              </option>
            </select>
          </label>

          <label>Battery SOC entity
            <select formControlName="batterySocEntityId">
              <option value="">-- brak --</option>
              <option *ngFor="let entity of filteredEntities" [value]="entity.entityId">
                {{ entity.friendlyName || entity.entityId }} ({{ entity.entityId }})
              </option>
            </select>
          </label>

          <label>Battery charge power entity
            <select formControlName="batteryChargePowerEntityId">
              <option value="">-- brak --</option>
              <option *ngFor="let entity of filteredEntities" [value]="entity.entityId">
                {{ entity.friendlyName || entity.entityId }} ({{ entity.entityId }})
              </option>
            </select>
          </label>

          <label>Battery discharge power entity
            <select formControlName="batteryDischargePowerEntityId">
              <option value="">-- brak --</option>
              <option *ngFor="let entity of filteredEntities" [value]="entity.entityId">
                {{ entity.friendlyName || entity.entityId }} ({{ entity.entityId }})
              </option>
            </select>
          </label>
        </ng-container>

        <label>Presence entity IDs (CSV) <input formControlName="presenceEntityIdsCsv"></label>
        <button type="submit" [disabled]="form.invalid">Zapisz</button>
      </form>
      <p *ngIf="saved">Zapisano.</p>
    </ng-container>
  `,
  styles: [`
    form {
      display: grid;
      gap: 0.75rem;
      max-width: 900px;
      background: #fff;
      padding: 1rem;
      border-radius: 0.5rem;
    }

    h3 {
      margin: 0.5rem 0 0;
    }

    .entity-picker {
      display: flex;
      flex-direction: column;
      gap: 0.35rem;
    }

    small {
      color: #4b5563;
      font-size: 0.8rem;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SettingsPageComponent {
  private readonly facade = inject(SettingsFacade);
  private readonly formBuilder = inject(FormBuilder);
  private readonly entitiesReloadSubject = new BehaviorSubject<void>(undefined);

  protected saved = false;
  protected readonly entityFilterControl = this.formBuilder.nonNullable.control('');
  protected readonly form = this.formBuilder.nonNullable.group({
    homeAssistantBaseUrl: ['', Validators.required],
    homeAssistantToken: ['', Validators.required],
    safetyMarginWatts: [500, [Validators.required, Validators.min(0)]],
    maxGridImportWatts: [200, [Validators.required, Validators.min(0)]],
    maxBatteryDischargeWatts: [100, [Validators.required, Validators.min(0)]],
    minBatterySocForStoragePercent: [40, [Validators.required, Validators.min(0), Validators.max(100)]],
    evaluationIntervalSeconds: [60, [Validators.required, Validators.min(10)]],
    commandCooldownSeconds: [60, [Validators.required, Validators.min(0)]],
    heartbeatIntervalSeconds: [45, [Validators.required, Validators.min(10)]],
    pvProductionEntityId: [''],
    gridImportEntityId: [''],
    gridExportEntityId: [''],
    batterySocEntityId: [''],
    batteryChargePowerEntityId: [''],
    batteryDischargePowerEntityId: [''],
    presenceEntityIdsCsv: ['']
  });

  protected readonly settings$ = this.facade.getSettings().pipe(
    tap((settings) => this.patchForm(settings))
  );

  private readonly allEntities$ = this.entitiesReloadSubject.pipe(
    switchMap(() =>
      this.facade.getHaEntities().pipe(
        map((entities) => entities.slice().sort((left, right) =>
          (left.friendlyName ?? left.entityId).localeCompare(right.friendlyName ?? right.entityId)
        )),
        catchError(() => of([] as HomeAssistantEntityDto[]))
      )
    ),
    shareReplay({ bufferSize: 1, refCount: true })
  );

  protected readonly filteredEntities$ = combineLatest([
    this.allEntities$,
    this.entityFilterControl.valueChanges.pipe(startWith(''))
  ]).pipe(
    map(([entities, filterText]) => this.filterEntities(entities, filterText))
  );

  protected save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const payload: SettingsDto = {
      homeAssistantBaseUrl: value.homeAssistantBaseUrl,
      homeAssistantToken: value.homeAssistantToken,
      safetyMarginWatts: value.safetyMarginWatts,
      maxGridImportWatts: value.maxGridImportWatts,
      maxBatteryDischargeWatts: value.maxBatteryDischargeWatts,
      minBatterySocForStoragePercent: value.minBatterySocForStoragePercent,
      evaluationIntervalSeconds: value.evaluationIntervalSeconds,
      commandCooldownSeconds: value.commandCooldownSeconds,
      heartbeatIntervalSeconds: value.heartbeatIntervalSeconds,
      pvProductionEntityId: value.pvProductionEntityId || null,
      gridImportEntityId: value.gridImportEntityId || null,
      gridExportEntityId: value.gridExportEntityId || null,
      batterySocEntityId: value.batterySocEntityId || null,
      batteryChargePowerEntityId: value.batteryChargePowerEntityId || null,
      batteryDischargePowerEntityId: value.batteryDischargePowerEntityId || null,
      presenceEntityIds: value.presenceEntityIdsCsv
        .split(',')
        .map((item) => item.trim())
        .filter((item) => item.length > 0)
    };

    this.facade.saveSettings(payload).subscribe({
      next: () => {
        this.saved = true;
      }
    });
  }

  protected refreshEntitiesFromHa(): void {
    this.facade.refreshHaEntities().subscribe({
      next: () => {
        this.entitiesReloadSubject.next();
      }
    });
  }

  private patchForm(settings: SettingsDto): void {
    this.form.patchValue({
      homeAssistantBaseUrl: settings.homeAssistantBaseUrl,
      homeAssistantToken: settings.homeAssistantToken,
      safetyMarginWatts: settings.safetyMarginWatts,
      maxGridImportWatts: settings.maxGridImportWatts,
      maxBatteryDischargeWatts: settings.maxBatteryDischargeWatts,
      minBatterySocForStoragePercent: settings.minBatterySocForStoragePercent,
      evaluationIntervalSeconds: settings.evaluationIntervalSeconds,
      commandCooldownSeconds: settings.commandCooldownSeconds,
      heartbeatIntervalSeconds: settings.heartbeatIntervalSeconds,
      pvProductionEntityId: settings.pvProductionEntityId ?? '',
      gridImportEntityId: settings.gridImportEntityId ?? '',
      gridExportEntityId: settings.gridExportEntityId ?? '',
      batterySocEntityId: settings.batterySocEntityId ?? '',
      batteryChargePowerEntityId: settings.batteryChargePowerEntityId ?? '',
      batteryDischargePowerEntityId: settings.batteryDischargePowerEntityId ?? '',
      presenceEntityIdsCsv: settings.presenceEntityIds.join(', ')
    });
  }

  private filterEntities(entities: HomeAssistantEntityDto[], filterText: string): HomeAssistantEntityDto[] {
    const query = filterText.trim().toLowerCase();
    if (query.length === 0) {
      return entities;
    }

    return entities.filter((entity) =>
      `${entity.friendlyName ?? ''} ${entity.entityId} ${entity.domain}`.toLowerCase().includes(query)
    );
  }
}