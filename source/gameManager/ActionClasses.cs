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
      targetPoint = Vector3.Inf;
      paramters = new SkillCheck.Paramters();
    }

    public bool canPerform;                   // Attacker has LoS, is in melee range, has an action available, has ammo, etc.
    public Vector3 targetPoint;               // Position to aim at. If the target is partly obstructed, this will be centered on the exposed area.
    public SkillCheck.Paramters paramters;    // Skill check 
  }

  public class ActionPlan
  {
    public ActionPlan() 
    { 
      returnCode = PlanReturnCode.idle;
    }

    public ActionCalculations calculations;
    public PlanReturnCode returnCode;
    public PawnController actor;
    public PawnController target;
    public Vector3 targetPoint;
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