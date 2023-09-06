using GameManagerStates;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameManagerStates
{
  public class ExecutorMove : GameManagerStates.IActionExecutor
  {
    private GameManager m_GameManager;
    private ActionPlan m_ActionPlan;

    void IActionExecutor.ExecutePlan(ActionPlan plan)
    {
      m_ActionPlan = plan;
    }

    public ExecutorMove(GameManager manager)
    {
      m_GameManager = manager;
    }

    ExecutorReturnCode IActionExecutor.DoUpdate()
    {
      throw new System.NotImplementedException();
    }
  }

} // namespace GameManagerStates
