using HomeManager.Domain.Models;

namespace HomeManager.Application.Services;

public interface IPowerManagerDecisionEngine
{
    PowerManagerDecisionResult Evaluate(PowerManagerInput input);
}
