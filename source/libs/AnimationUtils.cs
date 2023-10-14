using GameManagerStates;
using Godot;
using System;

public static class AnimationUtils
{
  public static AnimationPlayer CreateRangedAttackAnimation(ActionPlan plan)
  {
    if ( plan == null)
    {
      GD.PrintErr("tried to start an animation without a manager or a plan!");
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

    PawnUtils.Combat.AttackCalculations calcs = PawnUtils.Combat.CalculateRangedAttack(plan.actor, plan.target, plan.actor.GetWorld3D());
    AnimationPlayer player = new AnimationPlayer();
    Animation animation = new Animation();

    Vector3 attackOrigin = PawnUtils.Combat.GetRangedAttackOriginPoint(plan.actor);

    animation.Length = PawnUtils.Combat.GetProjectileDirectFlightTime(attackOrigin, );
    
    
    
    
    int trackIndex = animation.AddTrack(Animation.TrackType.Position3D);
    // TODO: calculate time for animation to play.
    // Setup playback.


    PackedScene projectileScene = ResourceLoader.Load<PackedScene>(statCard.weapon.projectileScene);
    Node3D projectile = projectileScene.Instantiate<Node3D>();
    if (projectile == null) 
    {
      GD.PrintErr("Failed to create projectile for a ranged attack animation!");
      return player;
    }

    return player;
  }
}
