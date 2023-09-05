using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace GameManagerStates
{
  public class MoveAction : MonoBehaviour, IGameState
  {
    private static float m_NavmeshCutoffDistance = 1.0f;
    private GameManager m_GameManager;
    private ScreenDecorator m_ScreenDecorator;

    public static ActionPlan GetDefaultActionPlan()
    {
      ActionPlan plan = new ActionPlan();
      plan.returnCode = StateReturnCode.idle;
      plan.actor = null;
      plan.target = null;
      plan.targetPoint = Vector3.negativeInfinity;
      plan.pawnAction = PawnAction.pass;
      return plan;
    }

    public MoveAction(GameManager manager)
    {
      if (manager == null)
        Debug.LogError("StateMoveAction does not have a game manager ref!");

      m_GameManager = manager;

      m_ScreenDecorator = gameObject.AddComponent<ScreenDecorator>();
    }

    private bool IsGameManagerOK()
    {
      if (m_GameManager == null)
        return false;

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

    ActionPlan IGameState.DoUpdate(PlayerInputs inputs)
    {
      ActionPlan plan = GetDefaultActionPlan();

      if (!IsGameManagerOK())
        return plan;

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

      NavMeshPath path = new NavMeshPath();
      if (!agent.CalculatePath(destination, path))
        return plan;

      // TODO: We now know the path is valid. Update the plan!
      // TODO: in the future this should check for pawn AP, etc.

      // Not checking for this in IsGameManagerOK because we may not always have a screen decorator
      if (m_ScreenDecorator == null)
        return plan;

      m_ScreenDecorator.DrawPath(path.corners);
      return plan;
    }

    void IGameState.Cleanup()
    {
      m_ScreenDecorator.ClearAllDecorations();
    }
  }

  } // namespace GameManager
  