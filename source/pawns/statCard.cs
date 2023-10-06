using Godot;
using System;

// This sucks, but I don't have a better solution right now.
public partial class StatCard : Resource
{
  [Export] public uint moveDistance;
  [Export] public uint armor;
  [Export] public uint skillDie;   // Number of sides on the skill die. Example: 6
  [Export] public uint skillBonus; // Bonus to the skill die. Only heroes or badasses get this. 

  // The skill die plus bonuses can be described by commone RPG notation. 
  // If Skill die is 6, and bonus is 1, then that is "1d6+1", or "roll a
  // 6 sided die, and add 1 to the total."
}
