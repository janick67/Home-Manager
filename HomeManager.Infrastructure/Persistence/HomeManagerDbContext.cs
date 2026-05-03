using HomeManager.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace HomeManager.Infrastructure.Persistence;

public sealed class HomeManagerDbContext : DbContext
{
    public HomeManagerDbContext(DbContextOptions<HomeManagerDbContext> options)
        : base(options)
    {
    }

    public DbSet<SettingEntity> Settings => Set<SettingEntity>();
    public DbSet<RoomEntity> Rooms => Set<RoomEntity>();
    public DbSet<RoomPresetMappingEntity> RoomPresetMappings => Set<RoomPresetMappingEntity>();
    public DbSet<ScheduleEntity> Schedules => Set<ScheduleEntity>();
    public DbSet<OverrideEntity> Overrides => Set<OverrideEntity>();
    public DbSet<ManagerStateEntity> ManagerStates => Set<ManagerStateEntity>();
    public DbSet<DecisionEntity> Decisions => Set<DecisionEntity>();
    public DbSet<HaCommandEntity> HaCommands => Set<HaCommandEntity>();
    public DbSet<LogEntryEntity> Logs => Set<LogEntryEntity>();
    public DbSet<HaEntityEntity> HaEntities => Set<HaEntityEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SettingEntity>(entity =>
        {
            entity.ToTable("settings");
            entity.HasKey(x => x.Id);
        });

        modelBuilder.Entity<RoomEntity>(entity =>
        {
            entity.ToTable("rooms");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.ClimateEntityId).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(150);
            entity.Property(x => x.ClimateEntityId).HasMaxLength(200);
            entity.Property(x => x.RoomType).HasMaxLength(50);
            entity.HasOne(x => x.PresetMapping)
                .WithOne(x => x.Room)
                .HasForeignKey<RoomPresetMappingEntity>(x => x.RoomId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RoomPresetMappingEntity>(entity =>
        {
            entity.ToTable("room_preset_mappings");
            entity.HasKey(x => x.RoomId);
            entity.Property(x => x.DefaultPreset).HasMaxLength(50);
            entity.Property(x => x.EcoPreset).HasMaxLength(50);
            entity.Property(x => x.NightPreset).HasMaxLength(50);
            entity.Property(x => x.AwayPreset).HasMaxLength(50);
            entity.Property(x => x.StoragePreset).HasMaxLength(50);
            entity.Property(x => x.NoPowerPreset).HasMaxLength(50);
        });

        modelBuilder.Entity<ScheduleEntity>(entity =>
        {
            entity.ToTable("schedules");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(150);
            entity.Property(x => x.Type).HasMaxLength(30);
            entity.Property(x => x.TargetType).HasMaxLength(30);
            entity.Property(x => x.PresenceMode).HasMaxLength(50);
            entity.Property(x => x.EnergyMode).HasMaxLength(50);
        });

        modelBuilder.Entity<OverrideEntity>(entity =>
        {
            entity.ToTable("overrides");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(150);
            entity.Property(x => x.TargetType).HasMaxLength(30);
            entity.Property(x => x.PresenceMode).HasMaxLength(50);
            entity.Property(x => x.EnergyMode).HasMaxLength(50);
        });

        modelBuilder.Entity<ManagerStateEntity>(entity =>
        {
            entity.ToTable("manager_states");
            entity.HasKey(x => x.ManagerName);
            entity.Property(x => x.ManagerName).HasMaxLength(120);
        });

        modelBuilder.Entity<DecisionEntity>(entity =>
        {
            entity.ToTable("decisions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ManagerName).HasMaxLength(80);
            entity.Property(x => x.ClimateEntityId).HasMaxLength(200);
            entity.Property(x => x.PreviousPreset).HasMaxLength(50);
            entity.Property(x => x.NewPreset).HasMaxLength(50);
            entity.Property(x => x.EnergyMode).HasMaxLength(50);
            entity.Property(x => x.PresenceMode).HasMaxLength(50);
        });

        modelBuilder.Entity<HaCommandEntity>(entity =>
        {
            entity.ToTable("ha_commands");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ClimateEntityId).HasMaxLength(200);
            entity.Property(x => x.Domain).HasMaxLength(50);
            entity.Property(x => x.Service).HasMaxLength(50);
        });

        modelBuilder.Entity<LogEntryEntity>(entity =>
        {
            entity.ToTable("logs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Level).HasMaxLength(20);
        });

        modelBuilder.Entity<HaEntityEntity>(entity =>
        {
            entity.ToTable("ha_entities");
            entity.HasKey(x => x.EntityId);
            entity.Property(x => x.EntityId).HasMaxLength(200);
            entity.Property(x => x.Domain).HasMaxLength(50);
            entity.Property(x => x.State).HasMaxLength(200);
        });
    }
}
