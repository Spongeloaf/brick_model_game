using GameManagerStates;
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

public enum PawnAction
{
  pass,
  move,
  attack,
}

namespace GameManagerStates
{
  public struct ActionPlan
  {
    public StateReturnCode returnCode;
    public PawnController actor;
    public PawnController[] target;
    public Vector3 targetPoint;
    public PawnAction pawnAction;
  }

  public enum StateReturnCode
  {
    idle,
    execute,
    abortState,
  }

  interface IGameState
  {
    ActionPlan DoUpdate(PlayerInputs inputs);
    void Cleanup();
  }

} // namespace GameManagerStates


// TODO: Make the statea use a virtual interface

public class GameManager : MonoBehaviour
{
  public GameObject m_SelectedUnit;
  public PlayerInputs m_PlayerInputs;

  private ManagerState m_ManagerState = ManagerState.idle;  // would be nice to get rid of this.
  private IGameState m_GameState;

  // Start is called before the first frame update
  void Start()
  {

  }

  // Update is called once per frame
  public void DoUpdate(PlayerInputs inputs)
  {
    HandleInputs(inputs);
    m_GameState.DoUpdate(inputs);
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
      m_GameState = new GameManagerStates.MoveAction(this);
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

  private IGameState CreateMoveState()
  {
    GameObject obj = new GameObject("GameManagerStates.MoveAction");
    GameManagerStates.MoveAction component = obj.AddComponent<GameManagerStates.MoveAction>();
    return component;
  }
}
