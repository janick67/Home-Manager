import { AsyncPipe, NgFor, NgIf } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RoomConfiguration, RoomType, ThermostatPresetMode } from '../../core/models/api.models';
import { RoomsFacade } from './rooms.facade';

const EMPTY_GUID = '00000000-0000-0000-0000-000000000000';

@Component({
  selector: 'app-rooms-page',
  standalone: true,
  imports: [AsyncPipe, NgFor, NgIf, ReactiveFormsModule],
  providers: [RoomsFacade],
  template: `
    <h2>Rooms</h2>
    <form [formGroup]="form" (ngSubmit)="submit()">
      <label>Nazwa <input formControlName="name"></label>
      <label>climateEntityId <input formControlName="climateEntityId"></label>
      <label>Room type
        <select formControlName="roomType">
          <option *ngFor="let type of roomTypes" [value]="type">{{ type }}</option>
        </select>
      </label>
      <label>Power watts <input type="number" formControlName="powerWatts"></label>
      <label>Priority <input type="number" formControlName="priority"></label>
      <label>Default preset
        <select formControlName="defaultPreset">
          <option *ngFor="let preset of presets" [value]="preset">{{ preset }}</option>
        </select>
      </label>
      <label>Eco preset
        <select formControlName="ecoPreset">
          <option *ngFor="let preset of presets" [value]="preset">{{ preset }}</option>
        </select>
      </label>
      <label>Storage preset
        <select formControlName="storagePreset">
          <option *ngFor="let preset of presets" [value]="preset">{{ preset }}</option>
        </select>
      </label>
      <label><input type="checkbox" formControlName="canStoreHeat"> Can store heat</label>
      <label><input type="checkbox" formControlName="allowPvBoost"> Allow PV boost</label>
      <label><input type="checkbox" formControlName="enabled"> Enabled</label>
      <button type="submit" [disabled]="form.invalid">Dodaj pokój</button>
    </form>

    <ng-container *ngIf="facade.state$ | async as state">
      <p *ngIf="state.status === 'loading'">Ładowanie listy pokoi...</p>
      <p *ngIf="state.status === 'error'">{{ state.errorMessage }}</p>
      <p *ngIf="state.status === 'loaded' && state.data?.length === 0">Brak pokoi.</p>

      <table *ngIf="state.status === 'loaded' && state.data && state.data.length > 0">
        <thead>
          <tr>
            <th>Nazwa</th>
            <th>Climate</th>
            <th>Typ</th>
            <th>Moc</th>
            <th>Priorytet</th>
            <th>Akcje</th>
          </tr>
        </thead>
        <tbody>
          <tr *ngFor="let room of state.data">
            <td>{{ room.name }}</td>
            <td>{{ room.climateEntityId }}</td>
            <td>{{ room.roomType }}</td>
            <td>{{ room.powerWatts }}</td>
            <td>{{ room.priority }}</td>
            <td><button type="button" (click)="delete(room.id)">Usuń</button></td>
          </tr>
        </tbody>
      </table>
    </ng-container>
  `,
  styles: [`
    form {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
      gap: 0.75rem;
      margin-bottom: 1rem;
      background: #fff;
      padding: 0.75rem;
      border-radius: 0.5rem;
    }

    table {
      width: 100%;
      border-collapse: collapse;
      background: #fff;
    }

    th,
    td {
      border-bottom: 1px solid #e5e7eb;
      text-align: left;
      padding: 0.5rem;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class RoomsPageComponent {
  protected readonly facade = inject(RoomsFacade);
  private readonly formBuilder = inject(FormBuilder);
  protected readonly presets: ThermostatPresetMode[] = ['None', 'Away', 'Comfort', 'Eco', 'Home', 'Sleep', 'Activity'];
  protected readonly roomTypes: RoomType[] = ['Bedroom', 'LivingRoom', 'Bathroom', 'Office', 'Utility', 'Buffer'];

  protected readonly form = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(2)]],
    climateEntityId: ['climate.', [Validators.required, Validators.pattern(/^climate\./)]],
    roomType: ['LivingRoom' as RoomType, Validators.required],
    powerWatts: [500, [Validators.required, Validators.min(1)]],
    priority: [1, [Validators.required, Validators.min(0)]],
    defaultPreset: ['Home' as ThermostatPresetMode, Validators.required],
    ecoPreset: ['Eco' as ThermostatPresetMode, Validators.required],
    storagePreset: ['Comfort' as ThermostatPresetMode, Validators.required],
    canStoreHeat: [true],
    allowPvBoost: [true],
    enabled: [true]
  });

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const room: RoomConfiguration = {
      id: EMPTY_GUID,
      name: value.name,
      climateEntityId: value.climateEntityId,
      currentTemperatureEntityId: null,
      roomType: value.roomType,
      powerWatts: value.powerWatts,
      priority: value.priority,
      canStoreHeat: value.canStoreHeat,
      allowPvBoost: value.allowPvBoost,
      allowNightOverride: true,
      enabled: value.enabled,
      presets: {
        defaultPreset: value.defaultPreset,
        ecoPreset: value.ecoPreset,
        nightPreset: value.ecoPreset,
        awayPreset: 'Away',
        storagePreset: value.storagePreset,
        noPowerPreset: 'Eco'
      },
      minOnTimeSeconds: 120,
      minOffTimeSeconds: 120,
      commandCooldownSeconds: 60
    };

    this.facade.createRoom(room).subscribe({
      next: () => this.form.patchValue({ name: '', climateEntityId: 'climate.' })
    });
  }

  protected delete(id: string): void {
    this.facade.deleteRoom(id).subscribe();
  }
}
