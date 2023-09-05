using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Windows;

internal enum ManagerState
{
  idle,
  moveAction,
}

public class GameManager : MonoBehaviour
{
  public GameObject m_SelectedUnit;
  public PlayerInputs m_PlayerInputs;
  public ScreenDecorator m_ScreenDecorator;

  private ManagerState m_ManagerState = ManagerState.idle;

  // Start is called before the first frame update
  void Start()
  {
    m_ScreenDecorator.Start();
  }

  // Update is called once per frame
  public void DoUpdate(PlayerInputs inputs)
  {
    m_PlayerInputs = inputs;
    HandleInputs(inputs);

    // Replace this with a table or somethin
    if (m_ManagerState == ManagerState.moveAction)
      StateMoveAction.CalculateMove(this);
  }

  private void HandleInputs(PlayerInputs inputs)
  {
    if (inputs is null)
    {
      return;
    }

    if (inputs.actions.Contains(ButtonEvents.commit))
    {
      DoSelection(inputs.playerCursor);
      
      // Don't allow other actions to be commited simultaneously!
      return;
    }

    if (inputs.actions.Contains(ButtonEvents.action1))
    {
      m_ManagerState = ManagerState.moveAction;
    }
  }

  private void DoSelection(RaycastHit cursor)
  {
    UnSelectUnit();
    
    if (cursor.collider != null) 
      SelectUnit(cursor.collider.transform.gameObject);
  }

  private void UnSelectUnit()
  {
    PawnUtils.Appearance.ClearHighlight(m_SelectedUnit);
    m_SelectedUnit = null;
  }

  private void SelectUnit(GameObject unit)
  {
    if (unit == null)
      return;

    if (unit.layer != Globals.LayerInts.Pawns)
      return;

    PawnController controller = unit.GetComponent<PawnController>();
    if (controller == null)
      controller = unit.GetComponentInParent<PawnController>();

    if (controller == null)
      return;

    m_SelectedUnit = controller.gameObject;
    PawnUtils.Appearance.SetHighlight(unit, UnityEngine.Color.green);
  }
}

internal static class StateMoveAction
{
  private static float m_NavmeshCutoffDistance = 1.0f;

  public static bool IsGameManagerOK(GameManager manager)
  {
    if (manager == null)
      return false;

    if (manager.m_SelectedUnit == null)
    {
      Debug.LogWarning("GameStateMoveAction cannot plot a move if no unit selected.");
      return false;
    }

    if (manager.m_PlayerInputs == null)
    {
      Debug.LogWarning("GameStateMoveAction cannot plot a move without a cursor position.");
      return false;
    }

    return true;
  }

  public static void CalculateMove(GameManager manager)
  {
    if (!IsGameManagerOK(manager))
      return;

    Vector3 destination;
    NavMeshHit hit;
    if (NavMesh.SamplePosition(manager.m_PlayerInputs.playerCursor.point, out hit, m_NavmeshCutoffDistance, NavMesh.AllAreas))
    {
      destination = hit.position;
    }
    else
    {
      return;
    }

    NavMeshAgent agent = PawnUtils.Navigation.GetNavAgent(manager.m_SelectedUnit);
    if (agent == null)
      return;

    if (!agent.SetDestination(destination))
      return;

    // Not checking for this in IsGameManagerOK because we may not always have a screen decorator
    if (manager.m_ScreenDecorator == null)
      return;

    manager.m_ScreenDecorator.DrawPath(agent.path.corners);
  }
}