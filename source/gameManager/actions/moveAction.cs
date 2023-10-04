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
    public void Cleanup()
    {
      // nothing to do
    }

    public ActionPlan DoUpdate(in InputActions actions, PawnController selectedPawn)
    {
      ActionPlan plan = new ActionPlan();
      if (actions.cursorPosition == null)
        return plan;

      if (selectedPawn == null)
        plan.returnCode = PlanReturnCode.abortState;

      if (selectedPawn.m_statCard == null)
      {
        GD.PrintErr("Selected pawn has no stat card!");
        return plan;
      }


      // TODO next:
      //
      // Also put the pathfinding code there
      // Make sure we don't save the path into the nav agent (ask for path, then tell it to stop navigating)
      // Then Then refernce the stat card to see if we can go that far
      // update the plan
      // Hook up the move code to the executor

      Vector3[] path = PawnUtils.Navigation.GetNavigationPath(selectedPawn, actions.cursorPosition.position);
      bool lengthOk = Math.GetPathLength(path) <= (float)selectedPawn.m_statCard.moveDistance;

      if (lengthOk && actions.command == PlayerCommands.commit)
        plan.returnCode = PlanReturnCode.execute;

      // TODO: This where we should add in code to draw the move path

      return plan;
    }

    public void RegisterManager(GameManager manager)
    {
      // nothing to do
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

    void IActionExecutor.ExecutePlan(ActionPlan plan)
    {
      //m_ActionPlan = plan;

      //if (plan.path.Count() < 2)
      //{
      //  // todo
      //  throw new System.NotImplementedException();
      //}

      //m_PathLength = Math.GetPathLength(m_ActionPlan.path);
      //double travelTime = m_PathLength / m_TravelSpeed;
      //m_BeginMoveTime = Time.GetUnixTimeFromSystem();
      //m_FinishMoveTime = m_BeginMoveTime + travelTime;
      throw new NotImplementedException();
    }

    ExecutorReturnCode IActionExecutor.DoUpdate()
    {
      // TODO: make this not have to getNode() every frame
      //NavigationAgent3D navAgent = m_ActionPlan.actor.GetNode<NavigationAgent3D>("navAgent");
      //Vector3 newPosition = Math.LerpAlongPath(navAgent.GetCurrentNavigationPath(), 0.0f, 2f);

      //if (m_FinishMoveTime < Time.GetUnixTimeFromSystem())
      //  return ExecutorReturnCode.finished;

      //return ExecutorReturnCode.running;
      throw new NotImplementedException ();
    }
  }

} // namespace GameManagerStates
