using GameManagerStates;
using Godot;
using System;

public class PlannerAttack : IActionPlanner
{
  PawnController m_currentTarget;


  public void Cleanup()
  {
    throw new NotImplementedException();
  }

  public ActionPlan DoUpdate(in InputActions actions, PawnController selectedPawn)
  {
    // Ensure we only draw the red highlight for one frame,
    // otherwise targeting a different pawn would highlight both.
    if (m_currentTarget != null)
    {
      PawnUtils.Appearance.ClearOverheadText(m_currentTarget);
      PawnUtils.Appearance.ClearHighlight(m_currentTarget);
    }

    ActionPlan plan = new ActionPlan();
    if (selectedPawn == null || actions.command == PlayerCommands.cancel)
      plan.returnCode = PlanReturnCode.abortState;

    plan.pawnAction = PawnAction.attack;
    plan.actor = selectedPawn;
    PawnController target = PawnUtils.GetPawnAtRaycastHit(actions.cursorPosition);
    if (target == null || target == selectedPawn)
      return plan;

    plan.target = target;
    m_currentTarget = target;
    PawnUtils.Appearance.SetHighlightRed(m_currentTarget);
    PawnUtils.Appearance.SetOverheadText(m_currentTarget, "ATTACK");

    if (actions.command == PlayerCommands.commit)
      plan.returnCode = PlanReturnCode.execute;

    return plan;
  }

  public void RegisterDecorator(ScreenDecorator painter)
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