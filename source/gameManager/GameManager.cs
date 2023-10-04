using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using GameManagerStates;

internal enum GameState
{
  selectPawn,
  move,
  attack,
}

public partial class GameManager : Node3D
{
  private PawnController m_SelectedPawn;
  private World3D m_world;
  private InputActions m_InputActions;
  private IActionPlanner m_Planner;
  private IActionExecutor m_Executor;
  private GameState m_GameState;

  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    m_GameState = GameState.selectPawn;
    m_world = GetWorld3D();
  }

  public void DoUpdate(InputActions inputs)
  {
    if (m_Executor != null)
    {
      // TODO: handle return codes
      m_Executor.DoUpdate();
      return;
    }

    m_InputActions = inputs;
    HandleInputs();

    // Expected to be null during pawn selection
    if (m_Planner == null)
      return;


    ActionPlan plan = m_Planner.DoUpdate(m_InputActions, m_SelectedPawn);
    if (plan.returnCode == PlanReturnCode.abortState)
    {
      m_Planner.Cleanup();
      m_Planner = null;
      return;
    }

    if (plan.returnCode == PlanReturnCode.idle)
      return;

    if (m_Executor == null)
    {
      GD.PrintErr("Uh oh, you somehow have a planner but not an executor in GameManager.DoUpdate()!");
      return;
    }

    if (plan.returnCode == PlanReturnCode.execute)
      m_Executor.ExecutePlan(plan);
  }

  // move into executor
  private void DoPawnMove(RaycastHit3D cursor)
  {
    if (cursor == null || cursor.position == Vector3.Inf)
      return;

    // this could be improved by moving the path calc into the pawn controller so it's nav agent
    // everything, ensuring consistent behavior.
    Vector3[] path = CalculatePath(cursor.position);
    if (path.Length == 0 || m_SelectedPawn == null)
      return;

    m_SelectedPawn.StartMovement(path.Last());
  }

  // move into pawn utils?
  private Vector3[] CalculatePath(Vector3 point)
  {
    if (m_SelectedPawn == null || point.IsEqualApprox(Vector3.Inf))
      return new Vector3[0];

    return NavigationServer3D.MapGetPath(m_world.NavigationMap, m_SelectedPawn.Position, point, true);
  }

  private void HandleInputs()
  {
    switch (m_InputActions.command)
    {
      case PlayerCommands.commit:
        SelectPawn(m_InputActions.cursorPosition);
        break;

      case PlayerCommands.cancel:
        if (m_Planner == null)
          UnselectCurrentPawn();
        break;

      case PlayerCommands.move:
        m_Planner = new PlannerMove();
        m_Executor = new ExecutorMove();
        break;

      case PlayerCommands.attack:
        throw new NotImplementedException();
        break;

      case PlayerCommands.nothing:
      default:
        break;
    }
  }

  private void UnselectCurrentPawn()
  {
    if (m_SelectedPawn == null)
      return;

    PawnUtils.Appearance.ClearHighlight(m_SelectedPawn);
    m_SelectedPawn = null;
  }

  private void SelectPawn(RaycastHit3D cursor)
  {
    PawnController pawn = GetPawnUnderCursor(cursor);

    if (pawn == null)
      return;

    if (pawn != m_SelectedPawn)
      UnselectCurrentPawn();

    if (pawn == null)
    {
      return;
    }

    m_SelectedPawn = pawn;
    PawnUtils.Appearance.SetHighlight(m_SelectedPawn);
  }

  private PawnController GetPawnUnderCursor(RaycastHit3D hit)
  {
    if (hit == null)
      return null;

    if (hit.collider == null)
      return null;

    Type objType = hit.collider.GetType();
    if (objType == typeof(PawnController))
      return (PawnController)hit.collider;

    if (objType != typeof(CollisionShape3D))
      return null;

    CollisionShape3D collider = (CollisionShape3D)hit.collider;
    return collider.GetParent<PawnController>();
  }
}
