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

    PawnUtils.Combat.AttackCalculations calculations = PawnUtils.Combat.CalculateRangedAttack(actor, target, actor.GetWorld3D());
    if (calculations.canAttack == false)
      return "No line of sight!";

    StatCard actorStatCard = actor.GetStatCard();
    
    // TODO: figure out a better place and system for this.
    string die = "1d" + actorStatCard.skillDie;
    if (actorStatCard.skillBonus > 0)
      die += " +" + actorStatCard.skillBonus;
    else if (actorStatCard.skillBonus < 0)
      die += " " + actorStatCard.skillBonus;

    return PawnUtils.Combat.GetSkillCheckString((int)actorStatCard.weapon.useRating, calculations.modifier, die);
  }

  public void RegisterDecorator(ScreenDecorator painter)
  {
    // decorator not needed here
  }
}

public class ExecutorAttack : IActionExecutor
{
  ActionPlan m_plan;

  public ExecutorReturnCode DoUpdate()
  {
    if (m_plan == null)
      return ExecutorReturnCode.finished;

    // ????
    return ExecutorReturnCode.running;
  }

  public void ExecutePlan(in ActionPlan plan)
  {
    m_plan = plan;

    if (m_plan == null)
      GD.PrintErr("WARNING: ExecutorAttack got a null action plan!");

    // Create projectile
    // calculate travel time
  }

  void CompletePlan() 
  { 
    // resolve damage roll on target
  }
}