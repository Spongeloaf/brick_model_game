using GameManagerStates;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameManagerStates
{
  public class ExecutorMove : GameManagerStates.IActionExecutor
  {
    private GameManager m_GameManager;
    private ActionPlan m_ActionPlan;
    private float m_PathLength = 0f;
    private float m_BeginMoveTime = 0f;
    private float m_FinishMoveTime = 0f;
    private static float m_TravelSpeed = 5f; // This is a bullshit made up constant

    void IActionExecutor.ExecutePlan(ActionPlan plan)
    {
      m_ActionPlan = plan;

      if (plan.path.Count() < 2)
      {
        // todo
        throw new System.NotImplementedException();
      }

      m_PathLength = PawnUtils.Navigation.GetPathLength(m_ActionPlan.path);
      float travelTime = m_PathLength / m_TravelSpeed;
      m_BeginMoveTime = Time.time;
      m_FinishMoveTime = m_BeginMoveTime + travelTime;
    }

    public ExecutorMove(GameManager manager)
    {
      m_GameManager = manager;
    }

    ExecutorReturnCode IActionExecutor.DoUpdate()
    {
      PawnUtils.Navigation.LerpObjectAlongPath(m_ActionPlan.actor, m_ActionPlan.path, m_BeginMoveTime, m_FinishMoveTime);

      if (m_FinishMoveTime < Time.time)
        return ExecutorReturnCode.finished;

      return ExecutorReturnCode.running;
    }
  }

} // namespace GameManagerStates
