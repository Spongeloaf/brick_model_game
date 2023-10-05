public enum PlayerCommands
{
  commit,
  cancel,
  nothing,
  move,
  attack,
}

public struct InputActions
{
  public InputActions() 
  {
    cursorPosition = new RaycastHit3D();
    command = PlayerCommands.nothing;
  }

  public RaycastHit3D cursorPosition;
  public PlayerCommands command;
}