import { AsyncPipe, NgFor, NgIf } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { EnergyMode, PresenceMode, ScheduleDefinition, ScheduleTargetType, ScheduleType } from '../../core/models/api.models';
import { SchedulesFacade } from './schedules.facade';

const EMPTY_GUID = '00000000-0000-0000-0000-000000000000';

@Component({
  selector: 'app-schedules-page',
  standalone: true,
  imports: [AsyncPipe, NgFor, NgIf, ReactiveFormsModule],
  providers: [SchedulesFacade],
  template: `
    <h2>Schedules</h2>
    <form [formGroup]="form" (ngSubmit)="submit()">
      <label>Nazwa <input formControlName="name"></label>
      <label>Typ
        <select formControlName="type">
          <option *ngFor="let type of scheduleTypes" [value]="type">{{ type }}</option>
        </select>
      </label>
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
      <label>Start UTC <input type="datetime-local" formControlName="startUtc"></label>
      <label>End UTC <input type="datetime-local" formControlName="endUtc"></label>
      <label>Priority <input type="number" formControlName="priority"></label>
      <button type="submit" [disabled]="form.invalid">Dodaj harmonogram</button>
    </form>

    <ng-container *ngIf="facade.state$ | async as state">
      <p *ngIf="state.status === 'loading'">Ładowanie...</p>
      <p *ngIf="state.status === 'error'">{{ state.errorMessage }}</p>
      <p *ngIf="state.status === 'loaded' && state.data?.length === 0">Brak harmonogramów.</p>
      <ul *ngIf="state.status === 'loaded' && state.data">
        <li *ngFor="let schedule of state.data">
          {{ schedule.name }} ({{ schedule.type }} / {{ schedule.targetType }})
          <button type="button" (click)="delete(schedule.id)">Usuń</button>
        </li>
      </ul>
    </ng-container>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SchedulesPageComponent {
  protected readonly facade = inject(SchedulesFacade);
  private readonly formBuilder = inject(FormBuilder);
  protected readonly scheduleTypes: ScheduleType[] = ['OneTime', 'Recurring'];
  protected readonly targetTypes: ScheduleTargetType[] = ['Presence', 'Energy'];
  protected readonly presenceModes: PresenceMode[] = ['EveryoneHome', 'NobodyHome', 'Vacation', 'AdultsOnly', 'NoRysioAndKinga', 'WorkingInOffice', 'NotWorkingInOffice'];
  protected readonly energyModes: EnergyMode[] = ['NoGridPower', 'ExpensivePower', 'NormalPower', 'CheapPower', 'FreePower', 'SurplusPower', 'HighSurplusPower'];

  protected readonly form = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(2)]],
    type: ['OneTime' as ScheduleType, Validators.required],
    targetType: ['Presence' as ScheduleTargetType, Validators.required],
    presenceMode: ['WorkingInOffice' as PresenceMode, Validators.required],
    energyMode: ['NormalPower' as EnergyMode, Validators.required],
    startUtc: ['', Validators.required],
    endUtc: ['', Validators.required],
    priority: [1, [Validators.required, Validators.min(0)]]
  });

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const schedule: ScheduleDefinition = {
      id: EMPTY_GUID,
      name: value.name,
      enabled: true,
      type: value.type,
      targetType: value.targetType,
      presenceMode: value.targetType === 'Presence' ? value.presenceMode : null,
      energyMode: value.targetType === 'Energy' ? value.energyMode : null,
      startUtc: new Date(value.startUtc).toISOString(),
      endUtc: new Date(value.endUtc).toISOString(),
      daysOfWeek: value.type === 'Recurring' ? ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday'] : [],
      dailyStartTime: value.type === 'Recurring' ? '07:00:00' : null,
      dailyEndTime: value.type === 'Recurring' ? '15:00:00' : null,
      priority: value.priority
    };

    this.facade.createSchedule(schedule).subscribe();
  }

  protected delete(id: string): void {
    this.facade.deleteSchedule(id).subscribe();
  }
}
