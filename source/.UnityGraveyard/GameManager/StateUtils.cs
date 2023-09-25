using GameManagerStates;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameManagerStates
{
  public static class StateUtils
  {
    public static ActionPlan GetDefaultActionPlan()
    {
      ActionPlan plan = new ActionPlan();
      plan.returnCode = PlanReturnCode.idle;
      plan.actor = null;
      plan.target = null;
      plan.targetPoint = Vector3.negativeInfinity;
      plan.pawnAction = PawnAction.pass;
      return plan;
    }

  } // class StateUtils

} // namespace GameManagerStates

