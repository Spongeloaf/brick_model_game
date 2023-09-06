using GameManagerStates;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Windows;

public enum PawnAction
{
  pass,
  move,
  attack,
}

namespace GameManagerStates
{
  public class ActionPlan
  {
    public PlanReturnCode returnCode;
    public PawnController actor;
    public PawnController[] target;
    public Vector3 targetPoint;
    public PawnAction pawnAction;
    public Vector3[] path;
  }

  public enum PlanReturnCode
  {
    idle,
    execute,
    abortState,
  }

  interface IActionPlanner
  {
    ActionPlan DoUpdate();
    void Cleanup();
    void RegisterManager(GameManager manager);
  }

  public enum ExecutorReturnCode
  {
    running,
    finished,
  }

  interface IActionExecutor
  {
    void ExecutePlan(ActionPlan plan);
    ExecutorReturnCode DoUpdate();
  }

} // namespace GameManagerStates


// TODO: Make the state use a virtual interface

public class GameManager : MonoBehaviour
{
  public GameObject m_SelectedUnit;
  public PlayerInputs m_PlayerInputs;
  public GameObject m_PrefabStateMove;

  private IActionPlanner m_GameState;
  private IActionExecutor m_Executor;

  // Start is called before the first frame update
  void Start()
  {

  }

  // Update is called once per frame
  public void DoUpdate(PlayerInputs inputs)
  {
    if (m_Executor != null)
    {
      m_Executor.DoUpdate();
      return;
    }

    m_PlayerInputs = inputs;
    HandleInputs();

    // This will be null when the game is idle (i.e. waiting for a player to select a unit).
    if (m_GameState == null)
      return;

    ActionPlan plan = m_GameState.DoUpdate();
    if (plan.returnCode == PlanReturnCode.abortState)
    {
      m_GameState.Cleanup();
      m_GameState = null;
      return;
    }

    if (plan.returnCode == PlanReturnCode.idle)
      return;

    if (plan.returnCode == PlanReturnCode.execute)
      ExecuteAction(plan);
  }

  private void HandleInputs()
  {
    if (m_PlayerInputs is null)
      return;

    //switch (m_ManagerState)
    //{
    //  default:
    //  case ManagerState.idle:
    //    if (m_PlayerInputs.actions.Contains(ButtonEvents.commit))
    //      DoSelection(m_PlayerInputs.playerCursor);
    //    break;
    //  case ManagerState.moveAction:
    //    // Commit action will be handled by game state class. We only need to handle 
    //    break;

    //}

    // Only change selected units when in the idle state
    if (m_GameState != null)
      return;

    if (m_PlayerInputs.actions.Contains(ButtonEvents.commit))
    {
      DoSelection(m_PlayerInputs.playerCursor);

      // Don't allow other actions to be commited simultaneously!
      return;
    }

    if (m_PlayerInputs.actions.Contains(ButtonEvents.cancel))
    {
      UnSelectUnit();

      // Don't allow other actions to be commited simultaneously!
      return;
    }

    if (m_PlayerInputs.actions.Contains(ButtonEvents.action1))
      m_GameState = GetMovePlanner();
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

  private IActionPlanner GetMovePlanner()
  {
    GameManagerStates.PlannerMove moveState = GetComponentInChildren<GameManagerStates.PlannerMove>();
    if (moveState == null)
    {
      Debug.LogError("CreateMoveState() failed to find component in MoveAction object");
      return null;
    }

    IActionPlanner state = (IActionPlanner)moveState;
    state.RegisterManager(this);
    return state;
  }

  private void ExecuteMoveAction(in ActionPlan plan)
  {
    m_Executor = new GameManagerStates.ExecutorMove(this);
    m_Executor.ExecutePlan(plan);
  }

  private void ExecuteAction(in ActionPlan plan)
  {
    switch (plan.pawnAction)
    {
      case PawnAction.pass:
        break;
      case PawnAction.move:
        ExecuteMoveAction(plan);
        break;
      case PawnAction.attack:
        break;
      default:
        break;
    }
  }
}

