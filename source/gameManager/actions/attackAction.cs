using GameManagerStates;
using Godot;
using System;
using System.Linq;

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

    m_plan.calculations = PawnUtils.Combat.CalculateRangedAttack(actor, target);
    string hitString = _Utils.CreateSkillCheckString(m_plan.calculations.skillCheck, m_plan.calculations.canPerform);
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
  // Currently this only handles ranged attacks
  ActionPlan m_plan;
  AnimationPlayer m_player;
  Node m_parent;
  bool m_attack_success;

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

    if (m_plan.target != null && m_attack_success)
      m_plan.target.QueueFree();

    return ExecutorReturnCode.finished;
  }

  public void ExecutePlan(in ActionPlan plan, Node parent)
  {
    m_plan = plan;
    m_parent = parent;
    if (m_plan == null || m_parent == null)
      GD.PrintErr("WARNING: ExecutorAttack got a null action plan or parent!");

    ActionCalculations m_calculations = PawnUtils.Combat.CalculateRangedAttack(plan.actor, plan.target);
    m_attack_success = SkillCheck.Do(plan.calculations.skillCheck);
    m_attack_success = true;
    if (!m_attack_success)
      CalculateMissedShot();

    // I hate that we pass in a parent node, and still have to return a value.
    m_player = AnimationUtils.CreateRangedAttackAnimation(m_plan, m_parent);
  }

  private void CalculateMissedShot()
  {
    Vector3 missPoint = PawnUtils.Combat.GetMissedShotPoint(m_plan.target, m_plan.calculations.shotOrigin);
    Vector3 shotDirection = missPoint - m_plan.calculations.shotOrigin;
    Rid[] excludeSelf = { m_plan.actor.GetRid() };

    // We take the miss offset point and raycast it into the world to see what actually gets hit.
    RaycastHit3D hit = GameWorldUtils.DoRaycastInDirection(
      m_plan.actor.GetWorld3D(),
      m_plan.calculations.shotOrigin,
      shotDirection,
      Globals.projectileMaxDistance,
      excludeSelf);

    // Figure out if we hit another pawn instead
    PawnController pawn = PawnUtils.GetPawnAtRaycastHit2(hit);
    if (pawn != null)
    {
      m_attack_success = true;
      m_plan.target = pawn;
    }

    // At some point in the future, we should see if we hit another pawn instead.
    m_plan.calculations.impactPoint = hit.position;
  }

} // class ExecutorAttack

internal static class _Utils
{
  public static string CreateSkillCheckString(SkillCheck.Parameters skillCheck, bool canPerform)
  {
    if (canPerform == false)
      return "No line of sight!";


    string result = "Skill Check: " + skillCheck.useRating;
    if (skillCheck.useModifiers > 0)
      result += " + " + skillCheck.useModifiers;

    else if (skillCheck.useModifiers < 0)
      result += " " + skillCheck.useModifiers;

    string die = "1d" + skillCheck.skillDie;

    // Currently not using skill die modifiers
    //if (skillCheck.useModifiers > 0)
    //  die += " + " + skillCheck.useModifiers;
    //else if (skillCheck.useModifiers < 0)
    //  die += " " + skillCheck.useModifiers;

    result += " on " + die;
    return result;
  }
}