//using GameManagerStates;
//using Godot;
//using System;

//public class SelectPawnPlanner : IActionPlanner
//{
//  public ActionPlan DoUpdate(in InputActions actions, PawnController selectedPawn)
//  {
//    ActionPlan plan = new ActionPlan();

//    if (actions.command == PlayerCommands.commit)
//    {
//      plan.actor = GetPawnUnderCursor(actions.cursorPosition);
//      if (plan.actor != null)
//        plan.returnCode = PlanReturnCode.execute;
//    }
//    else if (actions.command == PlayerCommands.cancel)
//    {
//      plan.actor = null; // should already be null, but I like to be explicit about this.
//      plan.returnCode = PlanReturnCode.execute;
//    }

//    return plan;
//  }

//  public void Cleanup()
//  {
//    // nothing to do
//  }

//  public void RegisterManager(GameManager manager)
//  {
//    throw new NotImplementedException();
//  }



//}

//public class SelectPawnExecutor : IActionExecutor
//{
//  public ExecutorReturnCode DoUpdate()
//  {
//    // All the work is done in ExecutePlan()
//    return ExecutorReturnCode.finished;
//  }

//  public void ExecutePlan(ActionPlan plan)
//  {
    
//  }

  
//}