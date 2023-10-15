using GameManagerStates;
using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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

    ExecutorReturnCode IActionExecutor.DoUpdate()
    {
      if (m_ActionPlan == null || m_ActionPlan.actor == null)
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
    }

    public void Cleanup()
    {

    }
  }

} // namespace GameManagerStates
