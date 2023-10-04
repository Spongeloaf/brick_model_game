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
    cursorPosition = null;
    command = PlayerCommands.nothing;
  }

  public RaycastHit3D cursorPosition;
  public PlayerCommands command;
}