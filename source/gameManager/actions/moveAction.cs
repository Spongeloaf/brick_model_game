using GameManagerStates;
using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GameManagerStates
{
  public class ExecutorMove : GameManagerStates.IActionExecutor
  {
    private GameManager m_GameManager;
    private ActionPlan m_ActionPlan;
    private double m_PathLength = 0f;
    private double m_BeginMoveTime = 0f;
    private double m_FinishMoveTime = 0f;
    private static double m_TravelSpeed = 5f; // This is a bullshit made up constant

    void IActionExecutor.ExecutePlan(ActionPlan plan)
    {
      m_ActionPlan = plan;

      if (plan.path.Count() < 2)
      {
        // todo
        throw new System.NotImplementedException();
      }

      m_PathLength = Math.GetPathLength(m_ActionPlan.path);
      double travelTime = m_PathLength / m_TravelSpeed;
      m_BeginMoveTime = Time.GetUnixTimeFromSystem();
      m_FinishMoveTime = m_BeginMoveTime + travelTime;
    }

    public ExecutorMove(GameManager manager)
    {
      m_GameManager = manager;
    }

    ExecutorReturnCode IActionExecutor.DoUpdate()
    {
      // TODO: make this not have to getNode() every frame
      NavigationAgent3D navAgent = m_ActionPlan.actor.GetNode<NavigationAgent3D>("navAgent");
      Vector3 newPosition = Math.LerpAlongPath(navAgent.GetCurrentNavigationPath(), 0.0f, 2f);

      if (m_FinishMoveTime < Time.GetUnixTimeFromSystem())
        return ExecutorReturnCode.finished;

      return ExecutorReturnCode.running;
    }
  }

} // namespace GameManagerStates
