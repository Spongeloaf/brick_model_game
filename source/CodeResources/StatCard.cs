using Godot;
using System;

public enum WeaponTypes
{
  ranged,
  melee,
}

[GlobalClass]
public partial class StatCard : Node
{
  private const uint defaultMoveDistance = 20;
  private const uint defaultArmor = 4;
  private const uint defaultSkillDie = 6;
  private const uint defaultSkillBonus = 0;

  // I don't think we need this anymore.
  //public StatCard()
  //{
  //  // assumes a barehanded minifig with no specialties at all.
  //  WeaponType = WeaponTypes.melee;
  //  moveDistance = defaultMoveDistance;
  //  armor = defaultArmor;
  //  skillBonus = defaultSkillBonus;
  //  skillDie = defaultSkillDie;
  //}

  public override void _Ready()
  {

  }

  // 
  [Export] public WeaponTypes WeaponType { get; set; } = WeaponTypes.melee;
  [Export] public uint moveDistance { get; set; } = defaultMoveDistance;
  [Export] public uint armor { get; set; } = defaultArmor;

  // Number of sides on the skill die. Example: 6
  [Export] public uint skillDie { get; set; } = defaultSkillDie;

  // Bonus to the skill die. Only heroes or badasses get this. 
  [Export] public uint skillBonus { get; set; } = defaultSkillBonus;
}