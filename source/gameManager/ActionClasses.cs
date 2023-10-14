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
  public class ActionPlan
  {
    public ActionPlan() 
    { 
      returnCode = PlanReturnCode.idle;
    }

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
    void ExecutePlan(in ActionPlan plan, in GameManager manager);
    ExecutorReturnCode DoUpdate();
  }

} // namespace GameManagerStates