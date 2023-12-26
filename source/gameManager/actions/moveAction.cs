using Godot;
using System.Linq;
using BrickModelGame.source.pawns;

namespace GameManagerStates
{
  public class PlannerMove : IActionPlanner
  {
    ScreenDecorator m_PathPainter;

    public void Cleanup()
    {
      m_PathPainter.ClearPath();
    }

    public ActionPlan DoUpdate(in InputActions actions, PawnController selectedPawn)
    {
      ActionPlan plan = new ActionPlan();
      if (selectedPawn == null)
      {
        plan.returnCode = PlanReturnCode.abortState;
        return plan;
      }

      plan.actor = selectedPawn;
      plan.path = PawnUtils.Navigation.GetNavigationPath(selectedPawn, actions.cursorPosition.position);
      plan.path = Math.GetPathLimitedToLength(plan.path, selectedPawn.m_statCard.moveDistance);

      if (actions.command == PlayerCommands.commit)
        plan.returnCode = PlanReturnCode.execute;

      if (m_PathPainter != null)
        m_PathPainter.DrawPath(plan.path, selectedPawn.m_statCard.moveDistance);

      return plan;
    }

    public void RegisterDecorator(ScreenDecorator painter)
    {
      m_PathPainter = painter;
    }
  }

  public class ExecutorMove : IActionExecutor
  {
    private ActionPlan m_ActionPlan;
    private ulong m_timeout;
    private const ulong m_timepadding = 5000; // # milliseconds to pad the time estimation.

    ExecutorReturnCode IActionExecutor.DoUpdate()
    {
      if (m_ActionPlan == null || m_ActionPlan.actor == null)
        return ExecutorReturnCode.finished;

      // The pawn probably got stuck if the timer expired.
      if (Time.GetTicksMsec() > m_timeout)
        return ExecutorReturnCode.finished;

      if (m_ActionPlan.actor.IsNavigating())
        return ExecutorReturnCode.running;

      return ExecutorReturnCode.finished;
    }

    public void ExecutePlan(in ActionPlan plan, Node parent)
    {
      if (plan == null || parent == null) 
        return;

      if (plan.actor == null || plan.path.Length == 0)
        return;

      m_ActionPlan = plan;
      m_ActionPlan.actor.StartNavigation(m_ActionPlan.path.Last());
      ulong millisecondsToMove = GameWorldUtils.CalculateMoveTimeInMsec(plan);
      
      // In case the pawn gets stuck while moving, we set the timeout for however long
      // we think the move will take, plus a standard padding amount.
      m_timeout = Time.GetTicksMsec() + millisecondsToMove + m_timepadding;
    }

    public void Cleanup()
    {
      if (m_ActionPlan != null && m_ActionPlan.actor != null)
        m_ActionPlan.actor.FinishNavigation();
    }
  }

} // namespace GameManagerStates
