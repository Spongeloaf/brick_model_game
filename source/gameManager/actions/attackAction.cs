using GameManagerStates;
using Godot;
using System;

public class PlannerAttack : IActionPlanner
{
  public void Cleanup()
  {
    throw new NotImplementedException();
  }

  public ActionPlan DoUpdate(in InputActions actions, PawnController selectedPawn)
  {
    ActionPlan plan = new ActionPlan();
    if (selectedPawn == null)
      plan.returnCode = PlanReturnCode.abortState;

    plan.actor = selectedPawn;
    PawnController target = PawnUtils.GetPawnAtRaycastHit(actions.cursorPosition);
    if (target != null || target == selectedPawn)
      return plan;

    if (selectedPawn == null)
    plan.target = target;
    return plan;
  }

  public void RegisterDecorator(PathPainter painter)
  {
    throw new NotImplementedException();
  }
}

public class ExecutorAttack : IActionExecutor
{
  public ExecutorReturnCode DoUpdate()
  {
    throw new NotImplementedException();
  }

  public void ExecutePlan(in ActionPlan plan)
  {
    throw new NotImplementedException();
  }
}