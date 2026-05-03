import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import {
  DashboardDto,
  HomeAssistantEntityDto,
  LogEntryDto,
  ManualOverrideDefinition,
  PowerManagerDecisionResult,
  RoomConfiguration,
  ScheduleDefinition,
  SettingsDto
} from '../models/api.models';
import { ApiClientService } from './api-client.service';

@Injectable({ providedIn: 'root' })
export class HomeManagerApiService {
  private readonly api = inject(ApiClientService);

  getDashboard(): Observable<DashboardDto> {
    return this.api.get<DashboardDto>('dashboard');
  }

  getHaEntities(): Observable<HomeAssistantEntityDto[]> {
    return this.api.get<HomeAssistantEntityDto[]>('ha/entities');
  }

  refreshHaEntities(): Observable<{ refreshed: number }> {
    return this.api.post<{ refreshed: number }>('ha/refresh-entities', {});
  }

  getRooms(): Observable<RoomConfiguration[]> {
    return this.api.get<RoomConfiguration[]>('rooms');
  }

  createRoom(room: RoomConfiguration): Observable<RoomConfiguration> {
    return this.api.post<RoomConfiguration>('rooms', room);
  }

  updateRoom(room: RoomConfiguration): Observable<void> {
    return this.api.put<void>(`rooms/${room.id}`, room);
  }

  deleteRoom(id: string): Observable<void> {
    return this.api.delete<void>(`rooms/${id}`);
  }

  getSchedules(): Observable<ScheduleDefinition[]> {
    return this.api.get<ScheduleDefinition[]>('schedules');
  }

  createSchedule(schedule: ScheduleDefinition): Observable<ScheduleDefinition> {
    return this.api.post<ScheduleDefinition>('schedules', schedule);
  }

  updateSchedule(schedule: ScheduleDefinition): Observable<void> {
    return this.api.put<void>(`schedules/${schedule.id}`, schedule);
  }

  deleteSchedule(id: string): Observable<void> {
    return this.api.delete<void>(`schedules/${id}`);
  }

  getOverrides(): Observable<ManualOverrideDefinition[]> {
    return this.api.get<ManualOverrideDefinition[]>('overrides');
  }

  createOverride(overrideEntry: ManualOverrideDefinition): Observable<ManualOverrideDefinition> {
    return this.api.post<ManualOverrideDefinition>('overrides', overrideEntry);
  }

  deleteOverride(id: string): Observable<void> {
    return this.api.delete<void>(`overrides/${id}`);
  }

  evaluatePowerManager(sendCommands: boolean): Observable<PowerManagerDecisionResult> {
    return this.api.post<PowerManagerDecisionResult>(`managers/power/evaluate?sendCommands=${sendCommands}`, {});
  }

  getLatestDecisions(count = 20): Observable<PowerManagerDecisionResult[]> {
    return this.api.get<PowerManagerDecisionResult[]>('decisions/latest', { count });
  }

  getLogs(count = 100): Observable<LogEntryDto[]> {
    return this.api.get<LogEntryDto[]>('logs', { count });
  }

  getSettings(): Observable<SettingsDto> {
    return this.api.get<SettingsDto>('settings');
  }

  updateSettings(settings: SettingsDto): Observable<void> {
    return this.api.put<void>('settings', settings);
  }
}
