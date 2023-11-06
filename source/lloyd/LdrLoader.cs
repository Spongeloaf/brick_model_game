using Godot;
using LDraw;
using System;
using System.Collections.Generic;

public partial class LdrLoader : Node3D
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        string result = LDrawConfig.Instance.GetModelByFileName("brick");
        if (result == null)
            return;

        //string fileName = "C:\\dev\\brick_model_game\\models\\gangster_groups.ldr";

        var zz = LDrawConfig.Instance.GetSerializedPart(result);
        LDrawModel model = LDrawModel.Create("brick", zz);
        List<Node> createdNodes = new List<Node>();
        Node3D node = model.CreateMeshGameObject(System.Numerics.Matrix4x4.Identity, null, null, createdNodes);

        if (node == null)
        {
            GD.PrintErr("Error creating node from ldraw file");
            return;
        }

        // This is a shitty hack. 
        // For some fucking reason, I cannot set the owner at the time of node
        // creation. For fucks sake.
        foreach (Node n in createdNodes)
        {
            n.Owner = node;
        }

        node.PrintTreePretty();
        Error error = LDrawConfig.Instance.SaveNodeAsScene(node);
        if (error != Error.Ok)
            GD.PrintErr("Error saving scene: " + error.ToString());
        GetTree().Quit();
    }

}