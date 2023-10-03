using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public partial class GameManager : Node3D
{
  PawnController m_SelectedPawn;
  World3D m_world;
  PathRenderer m_PathRenderer;

  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    m_SelectedPawn = null;
    try
    {
      m_world = GetWorld3D();
      m_PathRenderer = GetNode<PathRenderer>("PathRenderer");
    }
    catch
    {
      GD.PrintErr("Selected pawn not found!");
    }
  }

  public void DoUpdate(InputActions inputs)
  {
    if (inputs.command == PlayerCommands.commit)
    {
      if (m_SelectedPawn == null)
        SelectPawn(inputs.cursorPosition);
      else
        DoPawnMove(inputs.cursorPosition);
      return;
    }

    if (inputs.command == PlayerCommands.cancel)
    {
      UnselectCurrentPawn();
    }
  }

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

  private Vector3[] CalculatePath(Vector3 point)
  {
    if (m_SelectedPawn == null || point.IsEqualApprox(Vector3.Inf))
      return new Vector3[0];

    return NavigationServer3D.MapGetPath(m_world.NavigationMap, m_SelectedPawn.Position, point, true);
  }

  private void SelectPawn(RaycastHit3D cursor)
  {
    PawnController pawn = GetPawnUnderCursor(cursor);

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

  private void UnselectCurrentPawn()
  {
    if (m_SelectedPawn == null)
      return;

    PawnUtils.Appearance.ClearHighlight(m_SelectedPawn);
    m_SelectedPawn = null;
  }
}
