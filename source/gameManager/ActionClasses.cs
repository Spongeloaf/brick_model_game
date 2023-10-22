using Godot;
using System.Collections.Generic;

public enum PawnAction
{
  pass,
  move,
  attack,
}

namespace GameManagerStates
{
  public struct ActionCalculations
  {
    public ActionCalculations()
    {
      canPerform = false;
      impactPoint = Vector3.Inf;
      shotOrigin = Vector3.Inf;
      skillCheck = new SkillCheck.Paramters();
    }

    public bool canPerform;                   // Attacker has LoS, is in melee range, has an action available, has ammo, etc.
    public Vector3 impactPoint;               // Position to aim at. If the target is partly obstructed, this will be centered on the exposed area.
    public Vector3 shotOrigin;                // Position from which the projectile will spawn (if the attack uses a projectile)
    public SkillCheck.Paramters skillCheck;   // Skill check 
  }

  public class ActionPlan
  {
    public ActionPlan() 
    { 
      returnCode = PlanReturnCode.idle;
    }

    // Please consider moving the calculations out of this class.
    // None of these calcs should ever be run outside of an executor.
    // The "Can perform" flag should be moved into the ActionPlan.
    public ActionCalculations calculations;
    public PlanReturnCode returnCode;
    public PawnController actor;
    public PawnController target;
    public Vector3 cursorPoint;
    public PawnAction pawnAction;
    public Vector3[] path;
  }

  public enum PlanReturnCode
  {
    idle,
    execute,
    abortState,
  }

  interface IActionPlanner
  {
    ActionPlan DoUpdate(in InputActions actions, PawnController selectedPawn);
    void Cleanup();
    void RegisterDecorator(ScreenDecorator painter); // do we still need this?
  }

  public enum ExecutorReturnCode
  {
    running,
    finished,
  }

  interface IActionExecutor
  {
    void ExecutePlan(in ActionPlan plan, Node parent);  // Parent: Any nodes created during execution will be children of this node.
    ExecutorReturnCode DoUpdate();
    void Cleanup();
  }

} // namespace GameManagerStates