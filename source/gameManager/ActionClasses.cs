using Godot;

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
    public PlanReturnCode returnCode;
    public PawnController actor;
    public PawnController[] target;
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
    ActionPlan DoUpdate();
    void Cleanup();
    void RegisterManager(GameManager manager);
  }

  public enum ExecutorReturnCode
  {
    running,
    finished,
  }

  interface IActionExecutor
  {
    void ExecutePlan(ActionPlan plan);
    ExecutorReturnCode DoUpdate();
  }

} // namespace GameManagerStates