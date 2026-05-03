using HomeManager.Domain.Models;

namespace HomeManager.Application.Services;

public interface IScheduleEvaluator
{
    ScheduleEvaluationState Evaluate(
        IReadOnlyCollection<ScheduleDefinition> schedules,
        IReadOnlyCollection<ManualOverrideDefinition> overrideEntries,
        bool isNightMode);
}
