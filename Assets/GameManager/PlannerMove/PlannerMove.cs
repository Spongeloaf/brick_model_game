using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace GameManagerStates
{
  public class PlannerMove : MonoBehaviour, IActionPlanner
  {
    private static float m_NavmeshCutoffDistance = 1.0f;
    private GameManager m_GameManager;
    private ScreenDecorator m_ScreenDecorator;

    public GameObject m_PrefabScreenDecorator;
    
    void Start()
    {
      GameObject obj = Instantiate(m_PrefabScreenDecorator,transform);
      m_ScreenDecorator = obj.GetComponent<ScreenDecorator>();
    }

    private bool IsGameManagerOK()
    {
      if (m_GameManager == null)
      {
        Debug.LogWarning("GameStateMoveAction does not have a game manager ref.");
        return false;
      }

      if (m_GameManager.m_SelectedUnit == null)
      {
        Debug.LogWarning("GameStateMoveAction cannot plot a move if no unit selected.");
        return false;
      }

      if (m_GameManager.m_PlayerInputs == null)
      {
        Debug.LogWarning("GameStateMoveAction cannot plot a move without a cursor position.");
        return false;
      }

      return true;
    }

    ActionPlan IActionPlanner.DoUpdate()
    {
      ActionPlan plan = StateUtils.GetDefaultActionPlan();

      if (!IsGameManagerOK())
        return plan;

      if (m_GameManager.m_PlayerInputs.actions.Contains(ButtonEvents.cancel))
      {
        // abort!
        plan.returnCode = PlanReturnCode.abortState;
        return plan;
      }  

      Vector3 destination;
      NavMeshHit hit;
      if (NavMesh.SamplePosition(m_GameManager.m_PlayerInputs.playerCursor.point, out hit, m_NavmeshCutoffDistance, NavMesh.AllAreas))
      {
        destination = hit.position;
      }
      else
      {
        return plan;
      }

      NavMeshAgent agent = PawnUtils.Navigation.GetNavAgent(m_GameManager.m_SelectedUnit);
      if (agent == null)
        return plan;

      agent.enabled = true;
      NavMeshPath path = new NavMeshPath();
      if (!agent.CalculatePath(destination, path))
      {
        agent.enabled = false;
        return plan;
      }

      // TODO: Get obstacle component and disable it
      agent.enabled = false;

      // Not checking for this in IsGameManagerOK because we may not always have a screen decorator
      if (m_ScreenDecorator == null)
        return plan;

      m_ScreenDecorator.DrawPath(path.corners);
      
      if (m_GameManager.m_PlayerInputs.actions.Contains(ButtonEvents.commit))
        PreparePlan(ref plan, path);

      return plan;
    }

    void PreparePlan(ref ActionPlan plan, in NavMeshPath path)
    {
      Debug.LogWarning("Not Implemented!");
      plan.returnCode = PlanReturnCode.execute;
      plan.pawnAction = PawnAction.move;
      plan.path = path.corners;
    }

    void IActionPlanner.Cleanup()
    {
      m_ScreenDecorator.ClearAllDecorations();
    }

    void IActionPlanner.RegisterManager(GameManager manager)
    {
      m_GameManager = manager;
      if (manager == null)
        Debug.LogError("StateMoveAction does not have a game manager ref!");
    }
  }

} // namespace GameManager
