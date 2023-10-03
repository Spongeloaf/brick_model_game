public enum PlayerCommands
{
  commit,
  cancel,
}

public struct InputActions
{
  public RaycastHit3D cursorPosition;
  public PlayerCommands command;
}