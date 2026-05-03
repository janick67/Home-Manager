import { AsyncPipe, NgFor, NgIf } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { EnergyMode, ManualOverrideDefinition, PresenceMode, ScheduleTargetType } from '../../core/models/api.models';
import { OverridesFacade } from './overrides.facade';

const EMPTY_GUID = '00000000-0000-0000-0000-000000000000';

@Component({
  selector: 'app-overrides-page',
  standalone: true,
  imports: [AsyncPipe, NgFor, NgIf, ReactiveFormsModule],
  providers: [OverridesFacade],
  template: `
    <h2>Overrides</h2>
    <form [formGroup]="form" (ngSubmit)="submit()">
      <label>Nazwa <input formControlName="name"></label>
      <label>Target
        <select formControlName="targetType">
          <option *ngFor="let target of targetTypes" [value]="target">{{ target }}</option>
        </select>
      </label>
      <label>Presence mode
        <select formControlName="presenceMode">
          <option *ngFor="let mode of presenceModes" [value]="mode">{{ mode }}</option>
        </select>
      </label>
      <label>Energy mode
        <select formControlName="energyMode">
          <option *ngFor="let mode of energyModes" [value]="mode">{{ mode }}</option>
        </select>
      </label>
      <label>Start UTC <input type="datetime-local" formControlName="startsAtUtc"></label>
      <label>End UTC <input type="datetime-local" formControlName="endsAtUtc"></label>
      <label>Priority <input type="number" formControlName="priority"></label>
      <button type="submit" [disabled]="form.invalid">Dodaj override</button>
    </form>

    <ng-container *ngIf="facade.state$ | async as state">
      <p *ngIf="state.status === 'loading'">Ładowanie...</p>
      <p *ngIf="state.status === 'error'">{{ state.errorMessage }}</p>
      <ul *ngIf="state.status === 'loaded' && state.data">
        <li *ngFor="let item of state.data">
          {{ item.name }} ({{ item.targetType }}) {{ item.startsAtUtc }} - {{ item.endsAtUtc }}
          <button type="button" (click)="delete(item.id)">Usuń</button>
        </li>
      </ul>
    </ng-container>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class OverridesPageComponent {
  protected readonly facade = inject(OverridesFacade);
  private readonly formBuilder = inject(FormBuilder);
  protected readonly targetTypes: ScheduleTargetType[] = ['Presence', 'Energy'];
  protected readonly presenceModes: PresenceMode[] = ['EveryoneHome', 'NobodyHome', 'Vacation', 'AdultsOnly', 'NoRysioAndKinga', 'WorkingInOffice', 'NotWorkingInOffice'];
  protected readonly energyModes: EnergyMode[] = ['NoGridPower', 'ExpensivePower', 'NormalPower', 'CheapPower', 'FreePower', 'SurplusPower', 'HighSurplusPower'];

  protected readonly form = this.formBuilder.nonNullable.group({
    name: ['', Validators.required],
    targetType: ['Presence' as ScheduleTargetType, Validators.required],
    presenceMode: ['NobodyHome' as PresenceMode, Validators.required],
    energyMode: ['NormalPower' as EnergyMode, Validators.required],
    startsAtUtc: ['', Validators.required],
    endsAtUtc: ['', Validators.required],
    priority: [10, [Validators.required, Validators.min(0)]]
  });

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const overrideEntry: ManualOverrideDefinition = {
      id: EMPTY_GUID,
      name: value.name,
      enabled: true,
      targetType: value.targetType,
      presenceMode: value.targetType === 'Presence' ? value.presenceMode : null,
      energyMode: value.targetType === 'Energy' ? value.energyMode : null,
      startsAtUtc: new Date(value.startsAtUtc).toISOString(),
      endsAtUtc: new Date(value.endsAtUtc).toISOString(),
      priority: value.priority
    };

    this.facade.createOverride(overrideEntry).subscribe();
  }

  protected delete(id: string): void {
    this.facade.deleteOverride(id).subscribe();
  }
}
