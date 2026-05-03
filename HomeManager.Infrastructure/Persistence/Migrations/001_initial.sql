CREATE TABLE IF NOT EXISTS settings (
    Id INT NOT NULL PRIMARY KEY,
    HomeAssistantBaseUrl VARCHAR(512) NOT NULL DEFAULT '',
    HomeAssistantToken TEXT NOT NULL,
    SafetyMarginWatts INT NOT NULL,
    MaxGridImportWatts INT NOT NULL,
    MaxBatteryDischargeWatts INT NOT NULL,
    MinBatterySocForStoragePercent INT NOT NULL,
    EvaluationIntervalSeconds INT NOT NULL,
    CommandCooldownSeconds INT NOT NULL,
    HeartbeatIntervalSeconds INT NOT NULL,
    PvProductionEntityId VARCHAR(200) NULL,
    GridImportEntityId VARCHAR(200) NULL,
    GridExportEntityId VARCHAR(200) NULL,
    BatterySocEntityId VARCHAR(200) NULL,
    BatteryChargePowerEntityId VARCHAR(200) NULL,
    BatteryDischargePowerEntityId VARCHAR(200) NULL,
    PresenceEntityIdsCsv TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS rooms (
    Id CHAR(36) NOT NULL PRIMARY KEY,
    Name VARCHAR(150) NOT NULL,
    ClimateEntityId VARCHAR(200) NOT NULL,
    CurrentTemperatureEntityId VARCHAR(200) NULL,
    RoomType VARCHAR(50) NOT NULL,
    PowerWatts INT NOT NULL,
    Priority INT NOT NULL,
    CanStoreHeat TINYINT(1) NOT NULL,
    AllowPvBoost TINYINT(1) NOT NULL,
    AllowNightOverride TINYINT(1) NOT NULL,
    Enabled TINYINT(1) NOT NULL,
    MinOnTimeSeconds INT NOT NULL,
    MinOffTimeSeconds INT NOT NULL,
    CommandCooldownSeconds INT NOT NULL,
    UNIQUE KEY UX_rooms_ClimateEntityId (ClimateEntityId)
);

CREATE TABLE IF NOT EXISTS room_preset_mappings (
    RoomId CHAR(36) NOT NULL PRIMARY KEY,
    DefaultPreset VARCHAR(50) NOT NULL,
    EcoPreset VARCHAR(50) NULL,
    NightPreset VARCHAR(50) NULL,
    AwayPreset VARCHAR(50) NULL,
    StoragePreset VARCHAR(50) NULL,
    NoPowerPreset VARCHAR(50) NULL,
    CONSTRAINT FK_room_preset_mappings_rooms FOREIGN KEY (RoomId) REFERENCES rooms(Id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS schedules (
    Id CHAR(36) NOT NULL PRIMARY KEY,
    Name VARCHAR(150) NOT NULL,
    Enabled TINYINT(1) NOT NULL,
    Type VARCHAR(30) NOT NULL,
    TargetType VARCHAR(30) NOT NULL,
    PresenceMode VARCHAR(50) NULL,
    EnergyMode VARCHAR(50) NULL,
    StartUtc DATETIME NULL,
    EndUtc DATETIME NULL,
    DaysOfWeekCsv VARCHAR(50) NOT NULL,
    DailyStartTime TIME NULL,
    DailyEndTime TIME NULL,
    Priority INT NOT NULL
);

CREATE TABLE IF NOT EXISTS overrides (
    Id CHAR(36) NOT NULL PRIMARY KEY,
    Name VARCHAR(150) NOT NULL,
    Enabled TINYINT(1) NOT NULL,
    TargetType VARCHAR(30) NOT NULL,
    PresenceMode VARCHAR(50) NULL,
    EnergyMode VARCHAR(50) NULL,
    StartsAtUtc DATETIME NOT NULL,
    EndsAtUtc DATETIME NOT NULL,
    Priority INT NOT NULL
);

CREATE TABLE IF NOT EXISTS manager_states (
    ManagerName VARCHAR(120) NOT NULL PRIMARY KEY,
    LastHeartbeatAtUtc DATETIME NULL,
    LastEvaluationAtUtc DATETIME NULL
);

CREATE TABLE IF NOT EXISTS decisions (
    Id CHAR(36) NOT NULL PRIMARY KEY,
    TimestampUtc DATETIME NOT NULL,
    ManagerName VARCHAR(80) NOT NULL,
    RoomId CHAR(36) NOT NULL,
    ClimateEntityId VARCHAR(200) NOT NULL,
    PreviousPreset VARCHAR(50) NULL,
    NewPreset VARCHAR(50) NOT NULL,
    EnergyMode VARCHAR(50) NOT NULL,
    PresenceMode VARCHAR(50) NOT NULL,
    AvailablePowerWatts INT NOT NULL,
    GridImportWatts INT NOT NULL,
    GridExportWatts INT NOT NULL,
    BatterySocPercent INT NOT NULL,
    BatteryDischargeWatts INT NOT NULL,
    Reason TEXT NOT NULL,
    CommandSent TINYINT(1) NOT NULL,
    CommandResult TEXT NULL,
    INDEX IX_decisions_TimestampUtc (TimestampUtc)
);

CREATE TABLE IF NOT EXISTS ha_commands (
    Id CHAR(36) NOT NULL PRIMARY KEY,
    TimestampUtc DATETIME NOT NULL,
    ClimateEntityId VARCHAR(200) NOT NULL,
    Domain VARCHAR(50) NOT NULL,
    Service VARCHAR(50) NOT NULL,
    PayloadJson TEXT NOT NULL,
    Success TINYINT(1) NOT NULL,
    ResultMessage TEXT NULL,
    INDEX IX_ha_commands_TimestampUtc (TimestampUtc)
);

CREATE TABLE IF NOT EXISTS logs (
    Id CHAR(36) NOT NULL PRIMARY KEY,
    TimestampUtc DATETIME NOT NULL,
    Level VARCHAR(20) NOT NULL,
    Message TEXT NOT NULL,
    Details TEXT NULL,
    INDEX IX_logs_TimestampUtc (TimestampUtc)
);

CREATE TABLE IF NOT EXISTS ha_entities (
    EntityId VARCHAR(200) NOT NULL PRIMARY KEY,
    Domain VARCHAR(50) NOT NULL,
    State VARCHAR(200) NOT NULL,
    FriendlyName VARCHAR(255) NULL,
    AttributesJson LONGTEXT NOT NULL,
    LastSeenAtUtc DATETIME NOT NULL,
    INDEX IX_ha_entities_Domain (Domain)
);
