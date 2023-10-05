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
  private PathPainter m_PathPainter;

  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    m_GameState = GameState.selectPawn;
    m_world = GetWorld3D();
    m_PathPainter = GetNode<PathPainter>("PathPainter");
  }

  public void DoUpdate(InputActions inputs)
  {
    if (m_Executor != null)
    {
      // TODO: handle return codes
      ExecutorReturnCode code = m_Executor.DoUpdate();
      if (code == ExecutorReturnCode.finished) 
        m_Executor = null;

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

    if (plan.returnCode == PlanReturnCode.execute)
      ExecutePlan(plan);
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
        m_Planner.RegisterDecorator(m_PathPainter);
        m_GameState = GameState.move;
        break;

      case PlayerCommands.attack:
        throw new NotImplementedException();
        m_GameState = GameState.attack;
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

  private PawnController GetPawnUnderCursor(in RaycastHit3D hit)
  {
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

  public void ExecutePlan(in ActionPlan plan)
  {
    switch (m_GameState)
    {
      case GameState.selectPawn:
        // nothing to do
        break;
      case GameState.move:
        m_Executor = new ExecutorMove();
        break;
      case GameState.attack:
        break;
      default:
        break;
    }

    if (m_Executor != null)
      m_Executor.ExecutePlan(plan);
  }
}
