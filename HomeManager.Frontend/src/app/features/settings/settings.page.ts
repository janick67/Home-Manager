import { AsyncPipe, NgIf } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { tap } from 'rxjs';
import { SettingsDto } from '../../core/models/api.models';
import { SettingsFacade } from './settings.facade';

@Component({
  selector: 'app-settings-page',
  standalone: true,
  imports: [AsyncPipe, NgIf, ReactiveFormsModule],
  providers: [SettingsFacade],
  template: `
    <h2>Settings</h2>

    <ng-container *ngIf="settings$ | async">
      <form [formGroup]="form" (ngSubmit)="save()">
        <label>HA Base URL <input formControlName="homeAssistantBaseUrl"></label>
        <label>HA Token <input formControlName="homeAssistantToken"></label>
        <label>Safety margin watts <input type="number" formControlName="safetyMarginWatts"></label>
        <label>Max grid import watts <input type="number" formControlName="maxGridImportWatts"></label>
        <label>Max battery discharge watts <input type="number" formControlName="maxBatteryDischargeWatts"></label>
        <label>Min battery SOC for storage <input type="number" formControlName="minBatterySocForStoragePercent"></label>
        <label>Evaluation interval <input type="number" formControlName="evaluationIntervalSeconds"></label>
        <label>Heartbeat interval <input type="number" formControlName="heartbeatIntervalSeconds"></label>
        <button type="submit" [disabled]="form.invalid">Zapisz</button>
      </form>
      <p *ngIf="saved">Zapisano.</p>
    </ng-container>
  `,
  styles: [`
    form {
      display: grid;
      gap: 0.75rem;
      max-width: 600px;
      background: #fff;
      padding: 1rem;
      border-radius: 0.5rem;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SettingsPageComponent {
  private readonly facade = inject(SettingsFacade);
  private readonly formBuilder = inject(FormBuilder);

  protected saved = false;
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
}
