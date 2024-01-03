using GameManagerStates;
using Godot;
using BrickModelGame.source.pawns;
using System;
using BrickModelGame.source.pawns.components.weapons;

public static class AnimationUtils
{
    private static string ProgrammaticAnimations = "ProgrammaticAnimations";
    private static AnimationLibrary m_Library = new AnimationLibrary();

    public static AnimationPlayer CreateRangedAttackAnimation(ActionPlan plan, Node parent)
    {
        // The parent node will recieve both the animation player and any created nodes as children.
        // We also only need the parent node here because the Node3D.LookAt function only works if the
        // node is already in a scene tree.
        AnimationPlayer player = CreatePlayerIfArgumentsAreSane(plan, parent);
        if (player == null)
            return null;

        // after this point we have enough to at least let an empty animation play, which is better than returrning null;
       
        Projectile projectile = GetProjectile(plan, parent);

        player.AddChild(projectile);
        parent.AddChild(player);
        player.Name = "AttackAnimationPlayer";
        projectile.GlobalPosition = plan.calculations.shotOrigin;
        projectile.LookAt(plan.calculations.impactPoint);

        Animation animation = new Animation();
        ComposeRangedAttackTracks(animation, plan, projectile);

        string animName = "fire";
        Error err = m_Library.AddAnimation(animName, animation);
        if (err != Error.Ok)
        {
            GD.PrintErr("Failed to add animation to library!");
            return player;
        }

        err = player.AddAnimationLibrary(ProgrammaticAnimations, m_Library);
        if (err != Error.Ok)
        {
            GD.PrintErr("Failed to add animation to library!");
            return player;
        }

        player.Play(ProgrammaticAnimations + "/" + animName);
        return player;
    }

    private static Projectile GetProjectile(ActionPlan plan, Node actor)
    {
        RangedWeapon weapon = plan.actor.GetRangedWeapon(plan.calculations.actionIndex);
        if (weapon == null)
        {
            OmniLogger.Error("Tried to start a ranged attack animation with a null weapon!");
            return GetDefaultProjectile();
        }
        else
        {
            return weapon.GetProjectile();
        }
    }

    private static Projectile GetDefaultProjectile()
    {
        try
        {
            PackedScene resource = ResourceLoader.Load<PackedScene>("res://source/pawns/components/weapons/defaultProjectile.tscn");
            if (resource == null)
            {
                OmniLogger.Error("Failed to load default projectile!");
                return new Projectile();
            }

            Projectile projectile = resource.Instantiate<Projectile>();
            if (projectile == null)
            {
                OmniLogger.Error("Failed to instance default projectile!");
                return new Projectile();
            }

            return projectile;
        }
        catch (Exception e)
        {
            OmniLogger.Error(e.ToString());
            return new Projectile();
        }
    }

    private static void ComposeRangedAttackTracks(Animation animation, ActionPlan plan, Projectile projectile)
    {
        StatCard statCard = plan.actor.GetStatCard();
        float poseLength = CalculatePawnRotationTime(plan.actor, plan.calculations.impactPoint);

        // These paths connect the animation tracks to the node in the scene tree
        string pathProjectile = projectile.GetPath() + ":global_position";
        string pathActor = plan.actor.GetPath() + ":rotation";
        Quaternion pawnPose = Math.LookingAt2D(plan.actor.Transform, plan.calculations.impactPoint);

        // TODO: BUG: The rotation time estimator is off. I think it's not using the correct node rotation value.

        // Pawn pose track. Rotates the pawn towards the target
        int trackIndex = animation.AddTrack(Animation.TrackType.Rotation3D);
        animation.TrackSetPath(trackIndex, pathActor);
        animation.TrackInsertKey(trackIndex, 0.0f, plan.actor.Transform.Basis.GetRotationQuaternion());
        animation.TrackInsertKey(trackIndex, poseLength, pawnPose);

        float shotTime = PawnUtils.Combat.GetProjectileDirectFlightTime(
          plan.calculations.shotOrigin,
          plan.calculations.impactPoint,
          projectile.m_statCard.projectileSpeed);

        // Projectile track. Moves the projectile along from the origin point to the target point.
        trackIndex = animation.AddTrack(Animation.TrackType.Position3D);
        animation.TrackSetPath(trackIndex, pathProjectile);
        animation.TrackInsertKey(trackIndex, poseLength, plan.calculations.shotOrigin);
        animation.TrackInsertKey(trackIndex, poseLength + shotTime, plan.calculations.impactPoint);
        animation.Length = poseLength + shotTime;

        // hide projectile until it's time to fire
        trackIndex = animation.AddTrack(Animation.TrackType.Value);
        animation.TrackSetPath(trackIndex, projectile.GetPath() + ":visible");
        animation.TrackInsertKey(trackIndex, 0.0f, false);
        animation.TrackInsertKey(trackIndex, poseLength, false);
        animation.TrackInsertKey(trackIndex, poseLength + 0.01, true);
    }

    private static AnimationPlayer CreatePlayerIfArgumentsAreSane(ActionPlan plan, Node parent)
    {
        if (plan == null || parent == null)
        {
            GD.PrintErr("tried to start an animation without a plan or a parent!");
            return null;
        }

        if (plan.actor == null || plan.target == null)
        {
            GD.PrintErr("Tried to start a ranged attack animation with a null actor or target!");
            return null;
        }

        StatCard statCard = plan.actor.GetStatCard();
        if (statCard == null)
        {
            GD.PrintErr("Tried to start a ranged attack animation with a null actor statcard");
            return null;
        }

        return new AnimationPlayer();
    }

    public static AnimationPlayer CreateMoveAnimation(ActionPlan plan, Node parent)
    {
        // The parent node will recieve both the animation player and any created nodes as children.
        // We also only need the parent node here because the Node3D.LookAt function only works if the
        // node is already in a scene tree.
        AnimationPlayer player = CreatePlayerIfArgumentsAreSane(plan, parent);
        if (player == null)
            return null;

        // turn to face point

        return player;
    }

    private static float CalculatePawnRotationTime(PawnController pawn, Vector3 target)
    {
        Transform3D inFrontofPawn = pawn.Transform.TranslatedLocal(Vector3.Forward);
        Vector3 inFrontOfPawn = inFrontofPawn.Origin;

        // We only want to rotate the pawn on the Y axis, so we need to eliminate
        // Y coordinate differences in the math.
        inFrontOfPawn.Y = pawn.GlobalPosition.Y;
        target.Y = pawn.GlobalPosition.Y;

        Vector3 pawnFacing = pawn.GlobalPosition.DirectionTo(inFrontOfPawn);
        Vector3 lookingAtTarget = pawn.GlobalPosition.DirectionTo(target);
        float radians = lookingAtTarget.AngleTo(pawnFacing);
        float time = radians / Globals.m_pawnRotationSpeed;
        return time;
    }


}
