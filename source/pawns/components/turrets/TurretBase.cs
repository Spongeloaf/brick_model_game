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
        Color m_debugColor = Colors.White;

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

            float random = SkillCheck.GetRandomInt(0, 1000) * 0.001f;
            m_debugColor = Color.FromHsv(random, 1.0f, 1.0f);
        }

        public override void _PhysicsProcess(double delta)
        {
            // The targetChanged flag prevents visual jitter whiile moving the cursor
            // because the UI and physics process can run at different frame rates.
            // So, sometimes the turret would try to aim with no cursor position,
            // and it jumps rapidly between the real target, and pointing at 0,0,0.
            if (!targetChanged)
                return;

            // Simnple debugging hack to diffeentiate a turret's pan from its tilt
            //if (m_gimbalAxis.X > 0.5)
            //{
            //    int i = 0;
            //}

            GameWorldUtils.FaceTargetOnAxis(m_globalSpaceTarget, m_gimbalAxis, this, m_gimbal, m_debugColor);
            targetChanged = false;
        }

        public FiringSolution TrySetTarget(Vector3 target)
        {
            // TODO: Implement a feature to check that turret can face the target, considering
            // any gun constraints
            // TODO: Think about LOS chekcing here. Maybe this should only return true if the
            // target is within gun constraints, ignoring LOS? Or maybe we should also check for LOS
            // while were here, doing the math anyway?
            FiringSolution solution = new();

            m_globalSpaceTarget = target;
            targetChanged = true;
            foreach (TurretBase turret in m_childTUrrets)
                turret.TrySetTarget(target);

            return solution;    //TODO: make this work properly
        }
    }
} // namespace BrickModelGame.source.pawns.components.turrets
