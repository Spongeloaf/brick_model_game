using GameManagerStates;
using Godot;
using Godot.Collections;
using System;
using System.Linq;

[GlobalClass, Icon("res://source/pawn/pawn.svg")]
public partial class PawnController : CharacterBody3D
{
	// move speed target is 2.5 seconds to move 5 game units
	// that works out to 2 meters per second.
	[Export]
	private float m_speed = 10f;

	[Export]
	private float m_snapToGroundDistance = 5.0f;

  private NavigationAgent3D m_navAgent;
	private CollisionShape3D m_collisionShape;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

	public override void _Ready()
	{
		m_navAgent = GetNode<NavigationAgent3D>("navAgent");

		if (m_navAgent == null)
			GD.PrintErr("Missing nav agent!");

		m_collisionShape = GetNode<CollisionShape3D>("collider");
		if (m_collisionShape == null)
			GD.PrintErr("Failed to find collider!");
  }

	public override void _PhysicsProcess(double delta)
	{
    if (!m_navAgent.IsNavigationFinished())
    {
      DoNavigation(delta);
      return;
    }
    
		// Only do gravity when the pawn is thrown, pushed, etc.
		Vector3 velocity = Velocity;
		if (!IsOnFloor())
			velocity.Y -= gravity * (float)delta;

		Velocity = velocity;
		MoveAndSlide();
	}

	public void StartMovement(in Vector3 target)
	{
		if (m_navAgent == null)
			return;

		m_navAgent.TargetPosition = target;
  }

	private void DoNavigation(double timeDelta) 
	{ 
		if (m_navAgent.IsNavigationFinished()) 
			return;

		Vector3 movement = m_navAgent.GetNextPathPosition();
		float speed = (float)timeDelta * m_speed;
    GlobalPosition = GlobalPosition.MoveToward(movement, speed);
		SnapMeshToGround();

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

		RaycastHit3D hit = generalHelpers.DoRaycast(
			GetWorld3D(), newPosition, Vector3.Down, m_snapToGroundDistance, exclusions);

		if (hit == null || hit.position == Vector3.Inf)
			return;

		newPosition.Y = hit.position.Y;
		m_collisionShape.GlobalPosition = newPosition;
	}
}
