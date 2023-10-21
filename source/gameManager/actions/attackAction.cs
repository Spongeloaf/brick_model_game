using GameManagerStates;
using Godot;
using System;
using System.Numerics;

public class PlannerAttack : IActionPlanner
{
  PawnController m_currentTarget;
  ActionPlan m_plan; 

  public void Cleanup()
  {
    ClearCurrentTarget();
  }

  public ActionPlan DoUpdate(in InputActions actions, PawnController selectedPawn)
  {
    if (m_plan == null) 
      m_plan = new ActionPlan(); 
    
    if (selectedPawn == null || actions.command == PlayerCommands.cancel)
      m_plan.returnCode = PlanReturnCode.abortState;

    m_plan.pawnAction = PawnAction.attack;
    m_plan.actor = selectedPawn;
    PawnController target = PawnUtils.GetPawnAtRaycastHit(actions.cursorPosition);
    if (target == null || target == selectedPawn)
    {
      ClearCurrentTarget();
      return m_plan;
    }

    if (target != m_currentTarget)
    {
      ClearCurrentTarget();
      SetCurtrentTarget(m_plan.actor, target);
    }

    m_plan.target = target;
    if (actions.command == PlayerCommands.commit)
      m_plan.returnCode = PlanReturnCode.execute;

    return m_plan;
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

    m_plan.calculations = PawnUtils.Combat.CalculateRangedAttack(actor, target, actor.GetWorld3D());
    string hitString = _Utils.CreateSkillString(actor, target, m_plan.calculations);
    PawnUtils.Decoration.SetOverheadText(m_currentTarget, hitString);
    PawnUtils.Decoration.SetHighlightRed(m_currentTarget);
    // GD.Print("TARGET SET");  // for debugging
  }

  public void RegisterDecorator(ScreenDecorator painter)
  {
    // decorator not needed here
  }
}

public class ExecutorAttack : IActionExecutor
{
  ActionPlan m_plan;
  AnimationPlayer m_player;
  Node m_parent;

  public void Cleanup()
  {
    if (m_player != null)
    {
      Godot.Collections.Array<Node> children = m_player.GetChildren();
      foreach (Node node in children) 
        node.QueueFree();

      m_player.QueueFree();
    }

    m_plan = null;
  }

  public ExecutorReturnCode DoUpdate()
  {
    if (m_plan == null || m_player == null)
      return ExecutorReturnCode.finished;

    if (m_player.IsPlaying())
      return ExecutorReturnCode.running;

    return ExecutorReturnCode.finished;
  }

  public void ExecutePlan(in ActionPlan plan, Node parent)
  {
    m_plan = plan;
    m_parent = parent;
    if (m_plan == null || m_parent == null)
      GD.PrintErr("WARNING: ExecutorAttack got a null action plan or parent!");

    // Do the math!

    m_player = AnimationUtils.CreateRangedAttackAnimation(m_plan, m_parent);
  }

} // class ExecutorAttack

internal static class _Utils
{
  public static string CreateSkillString(PawnController actor, PawnController target, ActionCalculations calculations)
  {
    if (actor == null || target == null)
      return "ACTOR OR TARGET NULL!";
  
    if (calculations.canPerform == false)
      return "No line of sight!";

    StatCard actorStatCard = actor.GetStatCard();

    // TODO: figure out a better place and system for this.
    string die = "1d" + actorStatCard.skillDie;
    if (actorStatCard.skillBonus > 0)
      die += " +" + actorStatCard.skillBonus;
    else if (actorStatCard.skillBonus < 0)
      die += " " + actorStatCard.skillBonus;

    return PawnUtils.Combat.GetSkillCheckString((int)actorStatCard.weapon.useRating, calculations.useModifier, die);
  }

}