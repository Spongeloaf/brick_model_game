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
    // These calculations are expected to be used by executors only. We should probably not use them in planners.
    public ActionCalculations()
    {
      impactPoint = Vector3.Inf;
      shotOrigin = Vector3.Inf;
      skillCheck = new SkillCheck.Parameters();
    }

    public bool canPerform;                   // Attacker has LoS, is in melee range, has an action available, has ammo, etc.
    public Vector3 impactPoint;               // Position to aim at. If the target is partly obstructed, this will be centered on the exposed area.
    public Vector3 shotOrigin;                // Position from which the projectile will spawn (if the attack uses a projectile)
    public SkillCheck.Parameters skillCheck;  // Skill check values
  }

  public class ActionPlan
  {
    public ActionPlan() 
    { 
      returnCode = PlanReturnCode.idle;
    }

    public ActionCalculations calculations;   // Skill check params, impact point, etc
    public PlanReturnCode returnCode;         // Indicates if the planner should run again next frame, abort, or commit, etc.
    public PawnController actor;              // The pawn performing the action
    public PawnController target;             // If the action is targeted at another pawn, this is the pawn
    public Vector3 cursorPoint;               // If the action is targeted at a point in the world, this is the point
    public PawnAction pawnAction;             // Enumerator that specifies which action is being taken
    public Vector3[] path;                    // A path for the action to follow. Could be a pawn move path, or projectile arc, etc.
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