export type ThermostatPresetMode = 'None' | 'Away' | 'Comfort' | 'Eco' | 'Home' | 'Sleep' | 'Activity';
export type EnergyMode = 'NoGridPower' | 'ExpensivePower' | 'NormalPower' | 'CheapPower' | 'FreePower' | 'SurplusPower' | 'HighSurplusPower';
export type PresenceMode = 'EveryoneHome' | 'NobodyHome' | 'Vacation' | 'AdultsOnly' | 'NoRysioAndKinga' | 'WorkingInOffice' | 'NotWorkingInOffice';
export type RoomType = 'Bedroom' | 'LivingRoom' | 'Bathroom' | 'Office' | 'Utility' | 'Buffer';
export type ScheduleType = 'OneTime' | 'Recurring';
export type ScheduleTargetType = 'Presence' | 'Energy';

export interface RoomPresetMapping {
  defaultPreset: ThermostatPresetMode;
  ecoPreset?: ThermostatPresetMode | null;
  nightPreset?: ThermostatPresetMode | null;
  awayPreset?: ThermostatPresetMode | null;
  storagePreset?: ThermostatPresetMode | null;
  noPowerPreset?: ThermostatPresetMode | null;
}

export interface RoomConfiguration {
  id: string;
  name: string;
  climateEntityId: string;
  currentTemperatureEntityId?: string | null;
  roomType: RoomType;
  powerWatts: number;
  priority: number;
  canStoreHeat: boolean;
  allowPvBoost: boolean;
  allowNightOverride: boolean;
  enabled: boolean;
  presets: RoomPresetMapping;
  minOnTimeSeconds: number;
  minOffTimeSeconds: number;
  commandCooldownSeconds: number;
}

export interface ScheduleDefinition {
  id: string;
  name: string;
  enabled: boolean;
  type: ScheduleType;
  targetType: ScheduleTargetType;
  presenceMode?: PresenceMode | null;
  energyMode?: EnergyMode | null;
  startUtc?: string | null;
  endUtc?: string | null;
  daysOfWeek: string[];
  dailyStartTime?: string | null;
  dailyEndTime?: string | null;
  priority: number;
}

export interface ManualOverrideDefinition {
  id: string;
  name: string;
  enabled: boolean;
  targetType: ScheduleTargetType;
  presenceMode?: PresenceMode | null;
  energyMode?: EnergyMode | null;
  startsAtUtc: string;
  endsAtUtc: string;
  priority: number;
}

export interface DecisionReason {
  code: string;
  message: string;
}

export interface RoomDecision {
  roomId: string;
  roomName: string;
  climateEntityId: string;
  previousPreset?: ThermostatPresetMode | null;
  targetPreset: ThermostatPresetMode;
  reason: DecisionReason;
  commandSent: boolean;
}

export interface PowerManagerDecisionResult {
  evaluatedAtUtc: string;
  effectiveEnergyMode: EnergyMode;
  effectivePresenceMode: PresenceMode;
  availablePowerWatts: number;
  roomDecisions: RoomDecision[];
}

export interface HomeAssistantEntityDto {
  entityId: string;
  state: string;
  friendlyName?: string | null;
  domain: string;
  attributes: Record<string, unknown>;
}

export interface DashboardDto {
  homeAssistantConnected: boolean;
  energyMode: EnergyMode;
  presenceMode: PresenceMode;
  pvProductionWatts: number;
  gridImportWatts: number;
  gridExportWatts: number;
  batterySocPercent: number;
  batteryChargeWatts: number;
  batteryDischargeWatts: number;
  availablePowerWatts: number;
  activeSchedules: number;
  activeOverrides: number;
  latestDecisions: PowerManagerDecisionResult[];
}

export interface SettingsDto {
  homeAssistantBaseUrl: string;
  homeAssistantToken: string;
  safetyMarginWatts: number;
  maxGridImportWatts: number;
  maxBatteryDischargeWatts: number;
  minBatterySocForStoragePercent: number;
  evaluationIntervalSeconds: number;
  commandCooldownSeconds: number;
  heartbeatIntervalSeconds: number;
  pvProductionEntityId?: string | null;
  gridImportEntityId?: string | null;
  gridExportEntityId?: string | null;
  batterySocEntityId?: string | null;
  batteryChargePowerEntityId?: string | null;
  batteryDischargePowerEntityId?: string | null;
  presenceEntityIds: string[];
}

export interface LogEntryDto {
  timestampUtc: string;
  level: string;
  message: string;
  details?: string | null;
}
