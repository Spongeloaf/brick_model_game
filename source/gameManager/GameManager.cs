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
  private ScreenDecorator m_PathPainter;
  private bool m_CHEAT_chainActions = false;

  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    m_GameState = GameState.selectPawn;
    m_world = GetWorld3D();
    m_PathPainter = GetNode<ScreenDecorator>("ScreenDecorator");
  }

  public void DoUpdate(InputActions inputs)
  {
    if (m_Executor != null)
    {
      ExecutorReturnCode code = m_Executor.DoUpdate();
      if (code == ExecutorReturnCode.finished)
        OnFinishedExecution();

      // Don't let other actions run while executing!
      return;
    }

    m_InputActions = inputs;
    
    // I had a good reason for running this function even if there is a planner active,
    // but I bloody well don't remember what it was.
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

  private void OnFinishedExecution()
  {
    RemoveExecutor();

    if (m_CHEAT_chainActions)
      return;

    RemovePlanner();
    UnselectCurrentPawn();
  }

  private void HandleInputs()
  {
    switch (m_InputActions.command)
    {
      case PlayerCommands.commit:
        if (m_Planner == null)
          SelectPawn(m_InputActions.cursorPosition);
        break;

      case PlayerCommands.cancel:
        if (m_Planner == null)
          UnselectCurrentPawn();
        else
          RemovePlanner();
        break;

      case PlayerCommands.move:
        m_Planner = new PlannerMove();
        m_Planner.RegisterDecorator(m_PathPainter);
        m_GameState = GameState.move;
        break;

      case PlayerCommands.attack:
        m_Planner = new PlannerAttack();
        m_GameState = GameState.attack;
        break;

      case PlayerCommands.nothing:
      default:
        break;
    }
  }

  private void RemoveExecutor()
  {
    if (m_Executor == null)
      return;

    m_Executor.Cleanup();
    m_Executor = null;
  }

  private void RemovePlanner() 
  {
    if (m_Planner == null)
      return;

    m_Planner.Cleanup();
    m_Planner = null;
  }

  private void UnselectCurrentPawn()
  {
    if (m_SelectedPawn == null)
      return;

    PawnUtils.Decoration.ClearHighlight(m_SelectedPawn);
    m_SelectedPawn = null;
  }

  private void SelectPawn(RaycastHit3D cursor)
  {
    PawnController pawn = PawnUtils.GetPawnAtRaycastHit(cursor);

    if (pawn == null)
      return;

    if (pawn != m_SelectedPawn)
      UnselectCurrentPawn();

    if (pawn == null)
      return;

    m_SelectedPawn = pawn;
    PawnUtils.Decoration.SetHighlightGreen(m_SelectedPawn);
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
        m_Executor = new ExecutorAttack();
        break;
      default:
        break;
    }

    if (m_Executor != null)
      m_Executor.ExecutePlan(plan, this);
  }
}
