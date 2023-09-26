using Godot;
using System;

public partial class PlayerController : Node
{
  private float screenRayDepth = 100f;
  private GameManager m_gameManager;

  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    // We're expecting that the manager is a sibling
    m_gameManager = GetNode<GameManager>("../GameManager");

    if (m_gameManager == null)
      GD.PrintErr("GameManager not found!");
  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(double delta)
  {
    InputActions inputActions = new InputActions();
    inputActions.cursorPosition = GetMousePosition();
    if (Input.IsActionJustReleased("commit"))
      inputActions.click = true;

    if (m_gameManager != null)
      m_gameManager.DoUpdate(inputActions);
  }

  Vector3 GetMousePosition()
  {
    // Ugh. Godot is built to have each object react to being clicked on individually.
    // I have two options:
    // 1. Code my own raycaster. Not difficult, but possibly anti-thetical to Godot's way of doing things.
    // 2. Use the built in event handler. More in line with Godot's philosophies, but I don't like how it
    //    couples my objects to thier owners. Ref: https://ask.godotengine.org/44088/best-handle-mouse-touch-clicks-instanced-collision-objects
    Vector2 screenPos = GetViewport().GetMousePosition();
    Camera3D camera = GetViewport().GetCamera3D();
    Vector3 from = camera.ProjectRayOrigin(screenPos);
    Vector3 to = from + camera.ProjectRayNormal(screenPos) * screenRayDepth;
    return to;
  }
}
