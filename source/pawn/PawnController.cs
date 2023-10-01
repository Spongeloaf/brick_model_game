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
  }



  private void DoNavigation_old(double timeDelta)
  {
    if (m_navAgent.IsNavigationFinished())
      return;

    //Vector3 movement = m_navAgent.GetNextPathPosition();
    //float speed = (float)timeDelta * m_speed;
    //  GlobalPosition = GlobalPosition.MoveToward(movement, speed);
    //SnapMeshToGround();


    // We have a few potential solutions:
    //
    // Option A:	MoveAndSlide() with physics. However, that is resulting in wierd rubber banding and may require tuning
    //						to get it right. For example, waypoins in the middle may need higher distance tolerance and once we're
    //						close to the end point, we may need to slow down.
    //
    // Option 2:	Go back to the GlobalPosition = GlobalPosition.MoveToward(movement, speed) option, but use a tether to
    //						move the pawn. So the nav agent will move a dummy object, and then we'll raycast to ground from that
    //						point and set the pawn there.
    //
    // Option C:	Hybrid. The move and slide method may produce smoother movement, but it does not solve the hovering
    //						problem. Maybe combining the move and slide method with the raycaster will work?

    // Moving pawn by physics forces. Very janky:
    //Vector3 moveDelta = GlobalPosition.MoveToward(movement, speed) - GlobalPosition;
    //Velocity = moveDelta;
    //MoveAndSlide();
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

    newPosition.Y = hit.position.Y;
    m_collisionShape.GlobalPosition = newPosition;
  }
}
