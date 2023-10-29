using Godot;
using LDraw;
using System;

public partial class LdrLoader : Node3D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
    LDrawConfig cfg = new LDraw.LDrawConfig();
		cfg.InitParts();

		string result = cfg.GetModelByFileName("gangster_groups");
		if (result == null)
			return;

    LDrawModel model = LDrawModel.Create("gangster_groups.ldr", result);
    Node3D node = model.CreateMeshGameObject(System.Numerics.Matrix4x4.Identity, null, this);
		
		if (node == null)
		{
      GD.PrintErr("Error creating node from ldraw file");
      return;
		}
		
		Error error = cfg.SaveNodeAsScene(node);
		if (error != Error.Ok)
      GD.PrintErr("Error saving scene: " + error.ToString());
	}
	
}
