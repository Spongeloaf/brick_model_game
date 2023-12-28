using Godot;
using System;

[GlobalClass, Icon("res://source/pawn/pawn.svg")]
public partial class Gimbal : Node3D
{
    [Export] public Vector3 gimbalAxis = Vector3.Up;
}
