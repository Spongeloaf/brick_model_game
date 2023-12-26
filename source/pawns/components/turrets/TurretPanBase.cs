using Godot;
using System.Collections.Generic;
using BrickModelGame.source.libs;

namespace BrickModelGame.source.pawns.components.turrets
{
    [GlobalClass, Icon("res://source/pawn/pawn.svg")]
    public partial class TurretPanBase : Area3D, ITurret
    {
        Vector3 m_globalSpaceTarget;
        List<ElevatedWeapon> m_elevatedWeapons = new List<ElevatedWeapon>();

        public override void _Ready()
        {
           m_elevatedWeapons = TreeUtils.FindDirecChildren<ElevatedWeapon>(this);
        }

        // Called when the node enters the scene tree for the first time.
        private void Aim()
        {
            // If we ever decide to use local coords when updating this, we need to make sure
            // only calculate when the target actually changes.
            // We get some jitter happening because the game doesn't seem to update the mouse
            // cusror position at the same frequency as the physics engine updates, so the
            // aim jumps from where the mouse is, to poinitng at zero (or infinity?) between frames.

            Vector3 panTarget = m_globalSpaceTarget;
            panTarget.Y = GlobalPosition.Y;
            LookAt(panTarget, Vector3.Up, true);

            foreach (ElevatedWeapon weapon in m_elevatedWeapons)
            {
                weapon.SetTarget(m_globalSpaceTarget);
            }
        }


        public bool TrySetTarget(Vector3 target)
        {
            m_globalSpaceTarget = target;
            Aim();
            return true;    //TODO: make this work properly
        }
    }
} // namespace BrickModelGame.source.pawns.components.turrets
