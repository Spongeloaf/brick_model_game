using GameManagerStates;
using Godot;
using Godot.Collections;
using System;
using System.Linq;

//internal class KinematicPathFollower
//{
//  public KinematicPathFollower(in Vector3[] global_Waypoints) { globalWaypoints = global_Waypoints; }

//  public Vector3 GetMovementVector(Vector3 globalPosition)
//  {
//    // Plan:
//    // Get the closest waypoint.
//    // Find the next waypoint after that (unless last)
//    // Set destination to that waypoint
//    // Calculate direction based on target point
//    // Calculate speed based on pawn characteristics.

//    Vector3 result = NavigationUtils.GetNextGlobalWaypoint(globalWaypoints, globalPosition);

//    return result;
//  }

//  public Vector3[] globalWaypoints;
//}

[GlobalClass, Icon("res://source/pawn/pawn.svg")]
public partial class PawnController : RigidBody3D
{


  // move speed target is 2.5 seconds to move 5 game units
  // that works out to 2 meters per second.
  [Export]
  private float m_speed = 20f;

  [Export]
  private float m_snapToGroundDistance = 5.0f;

  private NavigationAgent3D m_navAgent;
  private CollisionShape3D m_collisionShape;

  // Get the gravity from the project settings to be synced with RigidBody nodes.
  public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

  public override void _Ready()
  {
    FreezeMode = FreezeModeEnum.Kinematic;
    Freeze = false;
    m_navAgent = GetNode<NavigationAgent3D>("navAgent");
    AddConstantCentralForce(new Vector3(0f, -gravity, 0f));

    if (m_navAgent == null)
      GD.PrintErr("Missing nav agent!");

    m_collisionShape = GetNode<CollisionShape3D>("collider");
    if (m_collisionShape == null)
      GD.PrintErr("Failed to find collider!");
  }

  public override void _PhysicsProcess(double delta)
  {
    if (m_navAgent == null)
      return;

    if (m_navAgent.IsNavigationFinished())
    {
      
    }

    // Navigation requries the body be frozen.
    if (Freeze) 
      DoNavigation_StaticMode(delta);
  }

  private void FinishNavigation()
  {
    if (Freeze == false)
      return;

    Freeze = false;
    LinearVelocity = Vector3.Zero;
  }

  public void StartMovement(in Vector3 target)
  {
    if (m_navAgent == null)
      return;

    Freeze = true;
    m_navAgent.TargetPosition = target;
  }

  private void DoNavigation_StaticMode(double timeDelta)
  {
    Vector3 movement = m_navAgent.GetNextPathPosition();
    float speed = (float)timeDelta * m_speed;
    GlobalPosition = GlobalPosition.MoveToward(movement, speed);
    SnapMeshToGround();
  }

  private void SnapMeshToGround()
  {
    if (m_collisionShape == null)
      return;

    Vector3 newPosition = GlobalPosition;
    newPosition.Y += 1;
    Array<Rid> exclusions = new Array<Rid>();
    exclusions.Add(GetRid());

    RaycastHit3D hit = NavigationUtils.DoRaycast(
      GetWorld3D(), newPosition, Vector3.Down, m_snapToGroundDistance, exclusions);

    if (hit == null || hit.position == Vector3.Inf)
      return;

    float difference = GlobalPosition.Y - hit.position.Y;
    string str = string.Format("y diff: {0}", difference);
    GD.Print(str);

    newPosition.Y = hit.position.Y;
    GlobalPosition = newPosition;
  }
}
