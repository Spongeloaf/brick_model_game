using BrickModelGame.source.libs;
using Godot;
using System;
using BrickModelGame.source.pawns.components.weapons;


[GlobalClass, Icon("res://source/pawn/pawn.svg")]
public partial class RangedWeapon : Area3D
{
	private Projectile[] m_projectiles;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		m_projectiles = TreeUtils.FindDirectChildren<Projectile>(this).ToArray();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public Projectile GetProjectile()
	{
		if (m_projectiles.Length == 0)
			return null;

		// TODO: This should be a random selection, not just the first one.
		return m_projectiles[0];
    }
}
