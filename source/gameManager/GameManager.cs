using Godot;
using System;

public partial class GameManager : Node3D
{
  Node3D m_SelectedPawn;
  World3D m_world;
  PathRenderer m_PathRenderer;
  Vector3[] m_waypoints;

  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    m_waypoints = new Vector3[0];

    try
    {
      m_SelectedPawn = GetNode<Node3D>("pawn");
      m_world = GetWorld3D();
      m_PathRenderer = GetNode<PathRenderer>("PathRenderer");
    }
    catch
    {
      GD.PrintErr("Selected pawn not found!");
    }
  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  //public override void _Process(double delta)
  //{
  //}

  // TODO: Clean this shit up
  public void DoUpdate(InputActions inputs)
  {
    if (inputs.click)
    {
      m_waypoints = CalculatePath(inputs.cursorPosition);
      if (m_PathRenderer != null)
      {
        Vector3[] globalPts = Math.GloablizePoints(m_waypoints, this);
        m_PathRenderer.DrawPath_Global(m_waypoints);
        NavigationAgent3D agent =  m_SelectedPawn.GetNode<NavigationAgent3D>("navAgent");
        if (agent != null)
        {
          agent.TargetPosition = inputs.cursorPosition;
        }
      }
    }
  }

  private Vector3[] CalculatePath(Vector3 point)
  {
    if (m_SelectedPawn == null || point.IsEqualApprox(Vector3.Inf))
      return new Vector3[0];

    return NavigationServer3D.MapGetPath(m_world.NavigationMap, m_SelectedPawn.Position, point, true);
  }

  public PawnController TryToSelectUnit(RaycastHit3D hit)
  {
    throw new NotImplementedException();
  }
}
