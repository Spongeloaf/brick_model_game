using GameManagerStates;
using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
        plan.returnCode = PlanReturnCode.abortState;

      // TODO next:
      //
      // Also put the pathfinding code there
      // Make sure we don't save the path into the nav agent (ask for path, then tell it to stop navigating)
      // Then Then refernce the stat card to see if we can go that far
      // update the plan
      // Hook up the move code to the executor

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
    private GameManager m_GameManager;
    private ActionPlan m_ActionPlan;
    private double m_PathLength = 0f;
    private double m_BeginMoveTime = 0f;
    private double m_FinishMoveTime = 0f;
    private static double m_TravelSpeed = 5f; // This is a bullshit made up constant

    void IActionExecutor.ExecutePlan(in ActionPlan plan)
    {
      if (plan.actor == null)
        return;

      m_ActionPlan = plan;
      if (m_ActionPlan.path.Length == 0)
      {
        m_ActionPlan = null;
        return;
      }

      m_ActionPlan.actor.StartMovement(m_ActionPlan.path.Last());
    }

    ExecutorReturnCode IActionExecutor.DoUpdate()
    {
      if (m_ActionPlan == null || m_ActionPlan.actor == null)
        return ExecutorReturnCode.finished;

      if (m_ActionPlan.actor.IsNavigating())
        return ExecutorReturnCode.running;

      return ExecutorReturnCode.finished;
    }
  }

} // namespace GameManagerStates
