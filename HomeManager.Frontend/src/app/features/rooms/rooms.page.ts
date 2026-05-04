import { AsyncPipe, NgFor, NgIf } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { combineLatest, map, startWith } from 'rxjs';
import { HomeAssistantEntityDto, RoomConfiguration, RoomType, ThermostatPresetMode } from '../../core/models/api.models';
import { RoomsFacade, RoomsVm } from './rooms.facade';

const EMPTY_GUID = '00000000-0000-0000-0000-000000000000';

@Component({
  selector: 'app-rooms-page',
  standalone: true,
  imports: [AsyncPipe, NgFor, NgIf, ReactiveFormsModule],
  providers: [RoomsFacade],
  template: `
    <h2>Rooms</h2>
    <ng-container *ngIf="facade.state$ | async as state">
      <p *ngIf="state.status === 'loading'">Ładowanie listy pokoi...</p>
      <p *ngIf="state.status === 'error'">{{ state.errorMessage }}</p>

      <form [formGroup]="form" (ngSubmit)="submit()">
        <h3 *ngIf="editingRoomId; else createTitle">Edycja pokoju</h3>
        <ng-template #createTitle><h3>Nowy pokój</h3></ng-template>
        <label>Nazwa <input formControlName="name"></label>
        <div class="climate-picker">
          <label>Filtr termostatów
            <input [formControl]="climateFilterControl" placeholder="np. salon albo climate.salon">
          </label>
          <button type="button" (click)="refreshClimateEntities()">Odśwież encje HA</button>
        </div>
        <ng-container *ngIf="filteredClimateEntities$ | async as filteredClimateEntities">
          <label>climateEntityId
            <select formControlName="climateEntityId">
              <option value="" disabled>Wybierz termostat...</option>
              <option *ngFor="let entity of filteredClimateEntities" [value]="entity.entityId">
                {{ entity.friendlyName || entity.entityId }} ({{ entity.entityId }})
              </option>
            </select>
          </label>
          <small *ngIf="state.status === 'loaded'">Dopasowanych termostatów: {{ filteredClimateEntities.length }}</small>
        </ng-container>
        <small *ngIf="state.status === 'loaded' && state.data && state.data.climateEntities.length === 0">
          Brak encji climate. Kliknij „Odśwież encje HA”, aby pobrać listę termostatów.
        </small>
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

        <div class="form-actions">
          <button type="submit" [disabled]="form.invalid">
            {{ editingRoomId ? 'Zapisz zmiany' : 'Dodaj pokój' }}
          </button>
          <button *ngIf="editingRoomId" type="button" (click)="cancelEdit()">Anuluj edycję</button>
        </div>
      </form>

      <p *ngIf="state.status === 'loaded' && state.data && state.data.rooms.length === 0">Brak pokoi.</p>

      <table *ngIf="state.status === 'loaded' && state.data && state.data.rooms.length > 0">
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
          <tr *ngFor="let room of state.data.rooms">
            <td>{{ room.name }}</td>
            <td>{{ room.climateEntityId }}</td>
            <td>{{ room.roomType }}</td>
            <td>{{ room.powerWatts }}</td>
            <td>{{ room.priority }}</td>
            <td class="table-actions">
              <button type="button" (click)="startEdit(room)">Edytuj</button>
              <button type="button" (click)="delete(room.id)">Usuń</button>
            </td>
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

    .climate-picker {
      display: flex;
      flex-direction: column;
      gap: 0.35rem;
    }

    .form-actions,
    .table-actions {
      display: flex;
      gap: 0.5rem;
      align-items: center;
      flex-wrap: wrap;
    }

    h3 {
      grid-column: 1 / -1;
      margin: 0;
    }

    small {
      color: #4b5563;
      font-size: 0.8rem;
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
  protected readonly climateFilterControl = this.formBuilder.nonNullable.control('');
  protected editingRoomId: string | null = null;

  private readonly defaultFormValues = {
    name: '',
    climateEntityId: '',
    roomType: 'LivingRoom' as RoomType,
    powerWatts: 500,
    priority: 1,
    defaultPreset: 'Home' as ThermostatPresetMode,
    ecoPreset: 'Eco' as ThermostatPresetMode,
    storagePreset: 'Comfort' as ThermostatPresetMode,
    canStoreHeat: true,
    allowPvBoost: true,
    enabled: true
  };

  protected readonly filteredClimateEntities$ = combineLatest([
    this.facade.state$,
    this.climateFilterControl.valueChanges.pipe(startWith(''))
  ]).pipe(
    map(([state, filterText]) => this.filterClimateEntities(state, filterText))
  );

  protected readonly form = this.formBuilder.nonNullable.group({
    name: [this.defaultFormValues.name, [Validators.required, Validators.minLength(2)]],
    climateEntityId: [this.defaultFormValues.climateEntityId, [Validators.required]],
    roomType: [this.defaultFormValues.roomType, Validators.required],
    powerWatts: [this.defaultFormValues.powerWatts, [Validators.required, Validators.min(1)]],
    priority: [this.defaultFormValues.priority, [Validators.required, Validators.min(0)]],
    defaultPreset: [this.defaultFormValues.defaultPreset, Validators.required],
    ecoPreset: [this.defaultFormValues.ecoPreset, Validators.required],
    storagePreset: [this.defaultFormValues.storagePreset, Validators.required],
    canStoreHeat: [this.defaultFormValues.canStoreHeat],
    allowPvBoost: [this.defaultFormValues.allowPvBoost],
    enabled: [this.defaultFormValues.enabled]
  });

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const room: RoomConfiguration = {
      id: this.editingRoomId ?? EMPTY_GUID,
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

    if (this.editingRoomId) {
      this.facade.updateRoom(room).subscribe({
        next: () => this.cancelEdit()
      });
      return;
    }

    this.facade.createRoom(room).subscribe({
      next: () => this.resetFormForCreate()
    });
  }

  protected startEdit(room: RoomConfiguration): void {
    this.editingRoomId = room.id;
    this.climateFilterControl.setValue('');
    this.form.patchValue({
      name: room.name,
      climateEntityId: room.climateEntityId,
      roomType: room.roomType,
      powerWatts: room.powerWatts,
      priority: room.priority,
      defaultPreset: room.presets.defaultPreset,
      ecoPreset: room.presets.ecoPreset ?? room.presets.defaultPreset,
      storagePreset: room.presets.storagePreset ?? room.presets.defaultPreset,
      canStoreHeat: room.canStoreHeat,
      allowPvBoost: room.allowPvBoost,
      enabled: room.enabled
    });
  }

  protected cancelEdit(): void {
    this.editingRoomId = null;
    this.resetFormForCreate();
  }

  protected refreshClimateEntities(): void {
    this.facade.refreshClimateEntities().subscribe();
  }

  protected delete(id: string): void {
    this.facade.deleteRoom(id).subscribe({
      next: () => {
        if (this.editingRoomId === id) {
          this.cancelEdit();
        }
      }
    });
  }

  private resetFormForCreate(): void {
    this.form.reset(this.defaultFormValues);
  }

  private filterClimateEntities(state: { status: string; data?: RoomsVm }, filterText: string): HomeAssistantEntityDto[] {
    if (state.status !== 'loaded' || !state.data) {
      return [];
    }

    const query = filterText.trim().toLowerCase();
    if (query.length === 0) {
      return state.data.climateEntities;
    }

    return state.data.climateEntities.filter((entity) =>
      `${entity.friendlyName ?? ''} ${entity.entityId}`.toLowerCase().includes(query)
    );
  }
}