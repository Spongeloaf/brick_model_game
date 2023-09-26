#if TOOLS
using Godot;

[Tool]
public partial class CustomNode : EditorPlugin
{
  public override void _EnterTree()
  {
    // Initialization of the plugin goes here.
    // Add the new type with a name, a parent type, a script and an icon.
    var script = GD.Load<Script>("res://source/pawn/pawn.cs");
    var texture = GD.Load<Texture2D>("res://source/pawn/icon.png");
    AddCustomType("Pawn", "Node3D", script, texture);
  }

  public override void _ExitTree()
  {
    // Clean-up of the plugin goes here.
    // Always remember to remove it from the engine when deactivated.
    RemoveCustomType("Pawn");
  }
}
#endif