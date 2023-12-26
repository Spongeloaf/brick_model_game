using BrickModelGame.source.libs;
using Godot;
using System.Collections.Generic;

namespace BrickModelGame.source.pawns
{

    [GlobalClass, Icon("res://source/pawn/pawn.svg")]
    public partial class VehicleController : RigidBody3D, IDirectPawnController
    {
        private List<ITurret> m_turrets ;    // This should be replaced with some system of registering gamme components

        public override void _Ready()
        {
            m_turrets = TreeUtils.FindDirecChildren<ITurret>(this);
        }

        public void DoDirectAction(InputActions actions)
        {
            if (!actions.cursorPosition.DidCollide())
                return;

            foreach (ITurret turret in m_turrets)
                turret.TrySetTarget(actions.cursorPosition.position);
        }
    }
}
