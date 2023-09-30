using Godot;
using Godot.Collections;
using System;

public partial class PlayerController : Node
{
  private float screenRayDepth = 100f;
  private GameManager m_gameManager;
  private Camera3D m_camera;

  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    // We're expecting that the manager is a sibling
    m_gameManager = GetNode<GameManager>("../GameManager");
    if (m_gameManager == null)
      GD.PrintErr("GameManager not found!");

    m_camera = GetNode<Camera3D>("Camera3D");
    if (m_camera == null)
      GD.PrintErr("Could not find player camera!");
  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _PhysicsProcess(double delta)
  {
    InputActions inputActions = new InputActions();
    if (Input.IsActionJustReleased("commit"))
    {
      inputActions.LClick = true;
      inputActions.cursorPosition = GetMousePosition();
    }

    if (Input.IsActionJustReleased("commit"))
      inputActions.RClick = true;

    if (m_gameManager != null)
      m_gameManager.DoUpdate(inputActions);
  }

  RaycastHit3D GetMousePosition()
  {
    if (m_camera == null)
      return null;

    Vector2 screenPos = GetViewport().GetMousePosition();
    Vector3 from = m_camera.ProjectRayOrigin(screenPos);
    Vector3 to = from + m_camera.ProjectRayNormal(screenPos) * screenRayDepth;
    var spaceState = m_camera.GetWorld3D().DirectSpaceState;
    var query = PhysicsRayQueryParameters3D.Create(from, to);
    Dictionary result = spaceState.IntersectRay(query);
    return generalHelpers.ConstructRaycastHit3D(result);
  }
}
