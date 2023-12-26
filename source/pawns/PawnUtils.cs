using System;
using System.Collections.Generic;
using System.Linq;
using GameManagerStates;
using Godot;
using BrickModelGame.source.pawns;

public static class PawnUtils
{
  public static PawnController GetPawnAtRaycastHit(in RaycastHit3D hit)
  {
    if (hit.collider == null)
      return null;

    Type objType = hit.collider.GetType();
    if (objType == typeof(PawnController))
      return (PawnController)hit.collider;

    if (objType != typeof(CollisionShape3D))
      return null;

    CollisionShape3D collider = (CollisionShape3D)hit.collider;
    return collider.GetParent<PawnController>();
  }

  public static PawnController GetPawnAtRaycastHit2(in RaycastHit3D hit)
  {
    if (hit.collider == null)
      return null;

    Type objType = hit.collider.GetType();
    if (objType == typeof(PawnController))
      return (PawnController)hit.collider;

    if (objType != typeof(CollisionShape3D))
      return null;

    CollisionShape3D collider = (CollisionShape3D)hit.collider;
    return collider.GetParent<PawnController>();
  }

  public static class Decoration
  {
    private static PackedScene m_labelTemplate = ResourceLoader.Load<PackedScene>("res://source/pawns/decorators/TextLabel.tscn");

    public static void SetHighlightGreen(PawnController pawn)
    {
      Material mat = ResourceLoader.Load("res://assets/materials/pawnMaterialHighlighted_Green.tres") as Material;
      SetMaterial(pawn, mat);
    }

    public static void SetHighlightRed(PawnController pawn)
    {
      Material mat = ResourceLoader.Load("res://assets/materials/pawnMaterialHighlighted_Red.tres") as Material;
      SetMaterial(pawn, mat);
    }

    public static void ClearHighlight(PawnController pawn)
    {
      Material mat = ResourceLoader.Load("res://assets/materials/pawnMaterial.tres") as Material;
      SetMaterial(pawn, mat);
    }

    private static void SetMaterial(PawnController pawn, Material material)
    {
      if (pawn == null || material == null)
        return;

      MeshInstance3D mesh = null;
      try
      {
        mesh = pawn.GetNode<MeshInstance3D>("collider/mesh");
        if (mesh == null)
          return;
      }
      catch
      {
        GD.PrintErr("Exception raised while looking for child node inside SetHighlight()!");
        return;
      }

      mesh.MaterialOverride = material;
    }

    public static void SetOverheadText(PawnController pawn, in string text)
    {
      if (pawn == null)
        return;

      Node3D anchor = GetLabelPoint(pawn);
      if (anchor == null)
        return;

      Node labelNode = m_labelTemplate.Instantiate();
      anchor.AddChild(labelNode);

      GodotObject obj = (GodotObject)labelNode;
      obj.Set("text", text);
    }

    public static void ClearOverheadText(PawnController pawn)
    {
      if (pawn == null)
        return;

      Node3D anchor = GetLabelPoint(pawn);
      if (anchor == null)
        return;

      Node label = anchor.GetChild(0);
      if (label == null)
        return;

      //anchor.RemoveChild(label);
      label.QueueFree();
    }

    public static Node3D GetLabelPoint(PawnController pawn)
    {
      if (pawn == null)
        return null;

      return pawn.GetNode<Node3D>("LabelPoint");
    }

  } // class Appearance

  public static class Navigation
  {
    // Uses the Navigation server to get a nav path for the pawn's nav agent to the destination
    // The "_global" postfix indicates that the argument must be a global position, not a local one.
    public static Vector3[] GetNavigationPath(PawnController pawn, in Vector3 target_global)
    {
      if (pawn == null)
        return new Vector3[0];

      return NavigationServer3D.MapGetPath(pawn.GetWorld3D().NavigationMap, pawn.GlobalPosition, target_global, true);
    }
  } // class Navigation

  public static class Combat
  {
    private static RandomNumberGenerator m_randomNumbers = new RandomNumberGenerator();

    // Returns set of attack calculations that contains a bool flag for line of sight, and
    // a penalty value from 0 to -2, based on the proportion of/ the target array that is visible
    // from the sight point of the attacker.
    public static ActionCalculations CalculateRangedAttack(PawnController actor, PawnController target)
    {
      ActionCalculations result = new ActionCalculations();
      if (actor == null || target == null)
        return result;
      
      result.shotOrigin = GetRangedAttackOriginPoint(actor);
      Vector3[] targetArray = target.GetTargetPoints();
      Vector3 sightPoint = GetSightPoint(actor);

      if (targetArray.Length == 0 || sightPoint == Vector3.Inf)
        return result;

      List<Vector3> visiblePoints = new List<Vector3>();
      foreach (Vector3 point in targetArray)
      {
        RaycastHit3D hit = GameWorldUtils.DoRaycastPointToPoint(actor.GetWorld3D(), sightPoint, point);
        if (hit.collider == target)
          visiblePoints.Add(point);
      }

      // If the enemy is not visible at all, use the centroid of their target points. That way,
      // even if we can't attack, any attempt to aim or point at the target will make sense because
      // it points to the center of mass.
      //
      // If the enemy is visible, aim for the center of the exposed area. 
      if (visiblePoints.Count > 0)
        result.impactPoint = Math.GetCentroidPoint(visiblePoints);
      else
        result.impactPoint = Math.GetCentroidPoint(targetArray);

      if (visiblePoints.Count > 0)
        result.canPerform = true;

      int modifier = 0;
      if (visiblePoints.Count == 1)
        modifier = 2;

      else if (visiblePoints.Count == 2)
        modifier = 1;

      CalculateSkillCheckValues(actor, modifier, ref result.skillCheck);
      return result;
    }

    private static void CalculateSkillCheckValues(PawnController actor, int useModifier, ref SkillCheck.Parameters skill)
    {
      skill.useModifiers = useModifier;

      StatCard statCard = actor.GetStatCard();
      if (statCard == null) 
        return;

      skill.skillDie = (int)statCard.skillDie;
      skill.useRating = (int)statCard.weapon.useRating;
    }

    public static Vector3 GetSightPoint(PawnController pawn)
    {
      if (pawn == null)
        return Vector3.Inf;

      Node3D sightPoint = pawn.GetNode<Node3D>("SightPoint");
      if (sightPoint == null)
        return pawn.GlobalPosition;

      return sightPoint.GlobalPosition;
    }

    public static Vector3 GetRangedAttackOriginPoint(PawnController pawn)
    {
      // TODO: Make this find the barrel of the weapon
      return GetSightPoint(pawn);
    }

    // This function is basically a simple math operation (t = d / v), but it has sanity checking 
    // and smart fallback values, so please use this everywhere instead of doing the math yourself.
    public static float GetProjectileDirectFlightTime(in Vector3 from, in Vector3 to, in float speed)
    {
      // Picked this default value so if something goes wrong with the calculation
      // things will move slowly enough that it will cause an investiagtion, but not
      // so slow that the game is unplayeable (imagine a 10,000 second duration!).
      float result = 5.0f;

      if (speed < 0)
        return result;

      float distance = from.DistanceTo(to);

      if (distance > Globals.projectileMaxDistance)
        distance = Globals.projectileMaxDistance;

      float time = distance / speed;

      // time equal to or less than 0 should never happen, but keep the default if it does.
      if (time > 0)
        result = time;
      else
        GD.PrintErr("GetProjectileDirectFlightTime() somehow got a negative result");

      return result;
    }

    public static Vector3 GetMissedShotPoint(PawnController pawn, in Vector3 origin)
    {
      // Returns a vector3 that in a position that is near, but not colliding with,
      // the pawn. The return point can be used to draw a projectile trajectory from
      // the supplied origin point that looks like a missed shot.
      if (pawn == null)
        return Vector3.Inf;

      // Imagine a ring around the pawn, whose diameter is slightly greater than the
      // pawn height. The ring is perpendicular to the trajectory of the incoming shot.
      // We'll pick a point on that ring at random to be where the shot will land.
      // The caller can then use that returned point to raycast the actual impact spot
      // in the world.
      //
      // We could also randomly increase or decrease the diameter of the ring for variety,
      // if we wanted to.

      float radius = 0;
      Vector3[] targetPts = pawn.GetTargetPoints();

      if (targetPts.Length == 0)
      {
        GD.PrintErr("GetMissedShotPoint got a pawn with no target point!");
        targetPts = new Vector3[1] { pawn.GlobalPosition };
      }

      if (targetPts.Length == 1)
      {
        // Assume that it's a small target if it only has one point.
        GD.PrintErr("GetMissedShotPoint got a pawn with only one target point!");
        radius = 1;
      }

      if(targetPts.Length > 1)
      {
        // This gives us a diameter of 2x the distance between the furthes target points.
        // DANGEROUS ASSUMPTION: The points may not be in a straight line in the future
        radius = targetPts[0].DistanceTo(targetPts.Last());
        radius *= m_randomNumbers.RandfRange(1f, 2f);
      }

      Vector3 centroid = Math.GetCentroidPoint(targetPts);
      float randomAngle = m_randomNumbers.RandfRange(0.0f, MathF.Tau);
      Transform3D tfm = new Transform3D(new Basis(), centroid);
      tfm = tfm.LookingAt(origin, Vector3.Up);
      tfm = tfm.RotatedLocal(Vector3.Forward, randomAngle);
      tfm = tfm.TranslatedLocal(Vector3.Up * radius);
      return tfm.Origin;
    }

  } // class Combat

  //public static class Skills
  //{
  //  public static bool MakeASkillCheck(in ActionPlan) { }
  //}
}