using Godot;
using System;

namespace BrickModelGame.source.pawns.components.weapons
{
    [GlobalClass, Icon("res://source/pawn/pawn.svg")]
    public partial class Projectile : Area3D
    {
        public WeaponStatCard m_statCard;

        public override void _Ready()
        {
            m_statCard = new WeaponStatCard();
        }
    }
}
