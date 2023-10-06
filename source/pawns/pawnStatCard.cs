using Godot;
using System;

public enum WeaponType
{
  melee,
  ranged
}

// This sucks, but I don't have a better solution right now.
[GlobalClass, Icon("res://source/pawn/pawn.svg")]
public partial class PawnStatCard : Node
{
  public WeaponType weaponType;
  [Export] public uint moveDistance { get; set; }
  [Export] public uint armor { get; set; }
  [Export] public uint skillDie { get; set; }   // Number of sides on the skill die. Example: 6
  [Export] public uint skillBonus { get; set; } // Bonus to the skill die. Only heroes or badasses get this. 

  // The skill die plus bonuses can be described by commone RPG notation. 
  // If Skill die is 6, and bonus is 1, then that is "1d6+1", or "roll a
  // 6 sided die, and add 1 to the total."

  public override void _Ready() { }
}