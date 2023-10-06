//#if TOOLS
//using Godot;


//[Tool]
//public partial class CustomNode : EditorPlugin
//{
//  private const string pawn = "Pawn";
//  private const string statcard = "StatCard";

//  public override void _EnterTree()
//  {
//    RegisterPawnController();
//    RegisterStatCard();
//  }

//  public override void _ExitTree()
//  {
//    // Clean-up of the plugin goes here.
//    // Always remember to remove it from the engine when deactivated.
//    RemoveCustomType(pawn);
//    RemoveCustomType(statcard);
//  }

//  public void RegisterPawnController()
//  {
//    // Initialization of the plugin goes here.
//    // Add the new type with a name, a parent type, a script and an icon.
//    var script = GD.Load<Script>("res://source/pawn/pawn.cs");
//    var texture = GD.Load<Texture2D>("res://source/pawn/pawn.svg");
//    AddCustomType(pawn, "Node3D", script, texture);
//  }

//  private void RegisterStatCard() 
//  {
//    // Initialization of the plugin goes here.
//    // Add the new type with a name, a parent type, a script and an icon.
//    var script = GD.Load<Script>("res://source/pawns/statCard.cs");
//    var texture = GD.Load<Texture2D>("res://source/pawn/pawn.svg");
//    AddCustomType(statcard, "Node", script, texture);
//  }
//}
//#endif