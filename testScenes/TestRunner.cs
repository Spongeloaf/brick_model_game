using Godot;
using System;

public partial class TestRunner : Node
{
  public override void _Ready()
  {
    SkillCheck.RunTests();
  }


}
