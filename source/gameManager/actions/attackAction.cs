using GameManagerStates;
using Godot;
using System;

public class PlannerAttack : IActionPlanner
{
  PawnController m_currentTarget;


  public void Cleanup()
  {
    ClearCurrentTarget();
  }

  public ActionPlan DoUpdate(in InputActions actions, PawnController selectedPawn)
  {
    ActionPlan plan = new ActionPlan();
    if (selectedPawn == null || actions.command == PlayerCommands.cancel)
      plan.returnCode = PlanReturnCode.abortState;

    plan.pawnAction = PawnAction.attack;
    plan.actor = selectedPawn;
    PawnController target = PawnUtils.GetPawnAtRaycastHit(actions.cursorPosition);
    if (target == null || target == selectedPawn)
    {
      ClearCurrentTarget();
      return plan;
    }

    if (target != m_currentTarget)
    {
      ClearCurrentTarget();
      SetCurtrentTarget(plan.actor, target);
    }

    plan.target = target;

    if (actions.command == PlayerCommands.commit)
      plan.returnCode = PlanReturnCode.execute;

    return plan;
  }

  private void ClearCurrentTarget()
  {
    if (m_currentTarget == null)
      return;

    PawnUtils.Decoration.ClearOverheadText(m_currentTarget);
    PawnUtils.Decoration.ClearHighlight(m_currentTarget);
    m_currentTarget = null;
    // GD.Print("TARGET CLEARED");  // for debugging
  }

  private void SetCurtrentTarget(PawnController actor, PawnController target)
  {
    m_currentTarget = target;

    if (m_currentTarget == null || actor == null)
      return;

    string hitString = CalculateHitOdds(actor, target);
    PawnUtils.Decoration.SetOverheadText(m_currentTarget, hitString);
    PawnUtils.Decoration.SetHighlightRed(m_currentTarget);
    // GD.Print("TARGET SET");  // for debugging
  }

  public static string CalculateHitOdds(PawnController actor, PawnController target)
  {
    if (actor == null || target == null)
      return "ACTOR OR TARGET NULL!";

    StatCard actorStatCard = actor.GetStatCard();
    int penalty = PawnUtils.Combat.CalculateAimPenalty(actor, target, actor.GetWorld3D());
    int useRating = (int)actorStatCard.weapon.useRating;
    useRating += penalty; // The penalty is 0 or negative, so don't subtract it here!
    
    if (useRating < 0)
      useRating = 0;

    int dieValue = (int)actorStatCard.skillBonus + (int)actorStatCard.skillDie;
    return "Hit chance: " + useRating + "/" + dieValue;
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