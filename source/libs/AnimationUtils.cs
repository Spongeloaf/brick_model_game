using GameManagerStates;
using Godot;

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

    StatCard statCard = plan.actor.GetStatCard();
    
    // after this point we have enough to at least let an empty animation play, which is better than returrning null;
    PackedScene projectileScene = ResourceLoader.Load<PackedScene>(statCard.weapon.projectileScene);
    Node3D projectile = projectileScene.Instantiate<Node3D>();
    if (projectile == null) 
    {
      GD.PrintErr("Failed to create projectile for a ranged attack animation!");
      return null;
    }

    player.AddChild(projectile);
    parent.AddChild(player);
    player.Name = "AttackAnimationPlayer";
    projectile.GlobalPosition = plan.calculations.shotOrigin;
    projectile.LookAt(plan.calculations.impactPoint);

    Animation animation = new Animation();
    ComposeTracks(animation, plan, projectile);

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

  private static void ComposeTracks(Animation animation, ActionPlan plan, Node3D projectile)
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
      statCard.weapon.projectileSpeed);
    
    // Projectile track. Moves the projectile along from the origin point to the target point.
    trackIndex = animation.AddTrack(Animation.TrackType.Position3D);
    animation.TrackSetPath(trackIndex, pathProjectile);
    animation.TrackInsertKey(trackIndex, poseLength, plan.calculations.shotOrigin);
    animation.TrackInsertKey(trackIndex, poseLength + shotTime, plan.calculations.impactPoint);
    animation.Length = poseLength + shotTime;
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
    Transform3D inFrontofPawn = pawn.Transform.Translated(Vector3.Back);
    Vector3 inFrontOfPawn = inFrontofPawn.Origin;
    
    // We only want to rotate the pawn on the Y axis, so we need to eliminate
    // Y coordinate differences in the math.
    inFrontOfPawn.Y = pawn.GlobalPosition.Y;
    target.Y = pawn.GlobalPosition.Y;
    
    Vector3 pawnFacing = pawn.GlobalPosition.DirectionTo(inFrontOfPawn);
    Vector3 lookingAtTarget = pawn.GlobalPosition.DirectionTo(target);
    float radians = lookingAtTarget.AngleTo(pawnFacing);
    float time = radians / Globals.m_pawnRotationSpeed;

    if (time > 0.8) 
    {
      int i = 0;
    }

    return time;
  }


}
