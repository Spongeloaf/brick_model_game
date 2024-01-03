using Godot;

public enum WeaponTypes
{
    ranged,
    melee,
}

public struct WeaponStatCard
{
    private const uint defaultUseRating = 3;
    private const WeaponTypes defaultWeaponTypes = WeaponTypes.melee;
    private const string defaultRangedProjectile = "res://source/equipment/basicProjectile.tscn";
    private const float defaultProjectileSpeed = 30f;

    public WeaponStatCard() { }

    [Export] public WeaponTypes WeaponType { get; set; } = defaultWeaponTypes;
    [Export] public uint useRating { get; set; } = defaultUseRating;
    [Export] public string projectileScene { get; set; } = defaultRangedProjectile;
    [Export] public float projectileSpeed { get; set; } = defaultProjectileSpeed;
}

[GlobalClass]
public partial class StatCard : Node
{
    private const uint defaultMoveDistance = 20;
    private const float defaultMoveSpeed = 5f;
    private const uint defaultArmor = 4;
    private const uint defaultSkillDie = 6;
    private const uint defaultSkillBonus = 0;

    [Export] public float moveSpeed { get; set; } = defaultMoveSpeed;
    [Export] public uint moveDistance { get; set; } = defaultMoveDistance;
    [Export] public uint armor { get; set; } = defaultArmor;

    // Number of sides on the skill die. Example: 6
    [Export] public uint skillDie { get; set; } = defaultSkillDie;

    // Bonus to the skill die. Only heroes or badasses get this. 
    [Export] public uint skillBonus { get; set; } = defaultSkillBonus;
}