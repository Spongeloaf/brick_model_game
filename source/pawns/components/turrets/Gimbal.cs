using Godot;
using System;

[GlobalClass, Icon("res://source/pawn/pawn.svg")]
public partial class Gimbal : Node3D
{
    [Export] public Vector3 gimbalAxis = Vector3.Up;

    // Angular values stored in Degrees to be human-friendly
    // The angular limit measured counter-clockwise from the turrets forward axis
    [Export] public float rotationLimitCCWDegrees = 0.0f;

    // The angular limit measured clockwise from the turrets forward axis
    [Export] public float rotationLimitCWDegrees = 0.0f;

}
