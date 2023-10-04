public enum PlayerCommands
{
  commit,
  cancel,
  nothing,
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