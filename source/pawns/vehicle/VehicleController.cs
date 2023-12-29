using BrickModelGame.source.libs;
using BrickModelGame.source.pawns.components.turrets;
using Godot;
using System.Collections.Generic;

namespace BrickModelGame.source.pawns
{

    [GlobalClass, Icon("res://source/pawn/pawn.svg")]
    public partial class VehicleController : RigidBody3D, IDirectPawnController
    {
        private List<TurretBase> m_turrets ;    // This should be replaced with some system of registering gamme components

        public override void _Ready()
        {
            m_turrets = TreeUtils.FindDirectChildren<TurretBase>(this);
        }

        public void DoDirectAction(InputActions actions)
        {
            if (!actions.cursorPosition.DidCollide())
                return;

            foreach (TurretBase turret in m_turrets)
                turret.TrySetTarget(actions.cursorPosition.position);
        }
    }
}
