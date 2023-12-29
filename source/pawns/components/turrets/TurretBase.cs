using Godot;
using System.Collections.Generic;
using BrickModelGame.source.libs;
using BrickModelGame.source.CodeResources;
using BrickModelGame.source.pawns.components.weapons;

namespace BrickModelGame.source.pawns.components.turrets
{

    // A component of a turret that roates on one axis.
    // If a turret can both pan and tilt, it should have two of these, where one is a child of the other.
    [GlobalClass, Icon("res://source/pawn/pawn.svg")]
    public partial class TurretBase : Node3D
    {
        bool targetChanged = false;
        Vector3 m_globalSpaceTarget;
        Gimbal m_gimbal;    // child object that actually rotates the turret components
        Vector3 m_gimbalAxis = Vector3.Up;
        List<TurretBase> m_childTUrrets = new List<TurretBase>();

        public override void _Ready()
        {
            m_gimbal = GetNode<Gimbal>("Gimbal");
            if (m_gimbal == null)
            {
                OmniLogger.Error("Failed to find gimbal!");
                throw new System.Exception("Failed to find gimbal!");
            }

            m_childTUrrets = TreeUtils.FindDirectChildren<TurretBase>(m_gimbal);

            // Cached for performance. We never expect this to change at run time.
            m_gimbalAxis = m_gimbal.gimbalAxis;
        }

        public override void _PhysicsProcess(double delta)
        {
            if (!targetChanged)
                return;

            if (m_gimbalAxis.X > 0.5)
            {
                int i = 0;
            }

            GameWorldUtils.LookAtOnAxis(m_globalSpaceTarget, m_gimbalAxis, this, m_gimbal);
            targetChanged = false;
        }

        public FiringSolution TrySetTarget(Vector3 target)
        {
            // TODO: Implement a feature to check that turret can face the target, considering
            // any gun constraints
            // TODO: Think about LOS chekcing here. Maybe this should only return true if the
            // target is within gun constraints, ignoring LOS? Or mabe we 
            FiringSolution solution = new();


            m_globalSpaceTarget = target;
            targetChanged = true;
            foreach (TurretBase turret in m_childTUrrets)
                turret.TrySetTarget(target);

            return solution;    //TODO: make this work properly
        }
    }
} // namespace BrickModelGame.source.pawns.components.turrets
