using Godot;
using System;

[GlobalClass]
public partial class StatCard : Node
{
  [Export] public uint moveDistance { get; set; }
  [Export] public uint armor { get; set; }
  [Export] public uint skillDie { get; set; }   // Number of sides on the skill die. Example: 6
  [Export] public uint skillBonus { get; set; } // Bonus to the skill die. Only heroes or badasses get this. 
}