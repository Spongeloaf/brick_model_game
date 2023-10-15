using GameManagerStates;
using Godot;
using System;
using static Godot.Projection;

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

    PawnUtils.Combat.AttackCalculations calcs = PawnUtils.Combat.CalculateRangedAttack(plan.actor, plan.target, plan.actor.GetWorld3D());
    StatCard statCard = plan.actor.GetStatCard();
    Animation animation = new Animation();
    
    // after this point we have enough to at least let an empty animation play, which is better than returrning null;
    Vector3 attackOrigin = PawnUtils.Combat.GetRangedAttackOriginPoint(plan.actor);
    animation.Length = PawnUtils.Combat.GetProjectileDirectFlightTime(attackOrigin, calcs.targetPoint, statCard.weapon.projectileSpeed);

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
    projectile.GlobalPosition = attackOrigin;
    projectile.LookAt(calcs.targetPoint);
    string pathProjectile = player.Name + "/" + projectile.Name + ":global_position";

    int trackIndex = animation.AddTrack(Animation.TrackType.Position3D);
    animation.TrackSetPath(trackIndex, pathProjectile);
    animation.TrackInsertKey(trackIndex, 0.0f, attackOrigin);
    animation.TrackInsertKey(trackIndex, animation.Length, calcs.targetPoint);
    string animName = Time.GetTimeStringFromSystem();
    animName.Replace(':', '_');

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
}
