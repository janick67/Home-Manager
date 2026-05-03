using HomeManager.Application.Abstractions;
using HomeManager.Domain.Enums;
using HomeManager.Domain.Models;

namespace HomeManager.Application.Services;

public sealed class ScheduleEvaluator : IScheduleEvaluator
{
    private readonly IClock _clock;
    private readonly IOverridePriorityResolver _overridePriorityResolver;

    public ScheduleEvaluator(IClock clock, IOverridePriorityResolver overridePriorityResolver)
    {
        _clock = clock;
        _overridePriorityResolver = overridePriorityResolver;
    }

    public ScheduleEvaluationState Evaluate(
        IReadOnlyCollection<ScheduleDefinition> schedules,
        IReadOnlyCollection<ManualOverrideDefinition> overrideEntries,
        bool isNightMode)
    {
        var now = _clock.UtcNow;

        var activeSchedules = schedules
            .Where(x => x.Enabled && IsScheduleActive(x, now))
            .ToList();

        var activeOverrides = overrideEntries
            .Where(x => x.Enabled && x.StartsAtUtc <= now && now <= x.EndsAtUtc)
            .ToList();

        var scheduledPresence = ResolveScheduledPresence(activeSchedules);
        var scheduledEnergy = ResolveScheduledEnergy(activeSchedules);
        var manualPresence = _overridePriorityResolver.ResolvePresenceOverride(activeOverrides);
        var manualEnergy = _overridePriorityResolver.ResolveEnergyOverride(activeOverrides);

        return new ScheduleEvaluationState(
            isNightMode,
            scheduledPresence,
            scheduledEnergy,
            manualPresence,
            manualEnergy);
    }

    private static PresenceMode? ResolveScheduledPresence(IReadOnlyCollection<ScheduleDefinition> schedules)
    {
        var presenceSchedules = schedules
            .Where(x => x.TargetType == ScheduleTargetType.Presence && x.PresenceMode.HasValue)
            .ToList();

        if (presenceSchedules.Count == 0)
        {
            return null;
        }

        if (presenceSchedules.Any(x => x.PresenceMode == PresenceMode.Vacation))
        {
            return PresenceMode.Vacation;
        }

        return presenceSchedules
            .OrderByDescending(x => x.Priority)
            .ThenBy(x => x.Name)
            .Select(x => x.PresenceMode)
            .FirstOrDefault();
    }

    private static EnergyMode? ResolveScheduledEnergy(IReadOnlyCollection<ScheduleDefinition> schedules)
    {
        return schedules
            .Where(x => x.TargetType == ScheduleTargetType.Energy && x.EnergyMode.HasValue)
            .OrderByDescending(x => x.Priority)
            .ThenBy(x => x.Name)
            .Select(x => x.EnergyMode)
            .FirstOrDefault();
    }

    private static bool IsScheduleActive(ScheduleDefinition schedule, DateTimeOffset now)
    {
        return schedule.Type switch
        {
            ScheduleType.OneTime => IsOneTimeScheduleActive(schedule, now),
            ScheduleType.Recurring => IsRecurringScheduleActive(schedule, now),
            _ => false
        };
    }

    private static bool IsOneTimeScheduleActive(ScheduleDefinition schedule, DateTimeOffset now)
    {
        if (!schedule.StartUtc.HasValue || !schedule.EndUtc.HasValue)
        {
            return false;
        }

        return schedule.StartUtc.Value <= now && now <= schedule.EndUtc.Value;
    }

    private static bool IsRecurringScheduleActive(ScheduleDefinition schedule, DateTimeOffset now)
    {
        if (schedule.DaysOfWeek.Count == 0 || !schedule.DailyStartTime.HasValue || !schedule.DailyEndTime.HasValue)
        {
            return false;
        }

        if (!schedule.DaysOfWeek.Contains(now.DayOfWeek))
        {
            return false;
        }

        var nowTime = TimeOnly.FromDateTime(now.DateTime);
        var start = schedule.DailyStartTime.Value;
        var end = schedule.DailyEndTime.Value;

        if (start <= end)
        {
            return nowTime >= start && nowTime < end;
        }

        return nowTime >= start || nowTime < end;
    }
}
