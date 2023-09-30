using GameManagerStates;
using Godot;
using System;
using System.Linq;

[GlobalClass, Icon("res://source/pawn/pawn.svg")]
public partial class PawnController : CharacterBody3D
{
	// move speed target is 2.5 seconds to move 5 game units
	// that works out to 2 meters per second.
	private const float m_speed = 200.0f;
  private NavigationAgent3D m_navAgent;
	private int m_nextWaypoint = 0; // index into the currentPath array
	private Vector3[] m_currentPath = null;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

	public override void _Ready()
	{
		m_navAgent = GetNode<NavigationAgent3D>("navAgent");

		if (m_navAgent == null)
			GD.PrintErr("Missing nav agent!");

		m_navAgent.Height = 1f;
		m_navAgent.PathHeightOffset = 0f;
	}

	public override void _PhysicsProcess(double delta)
	{
		DoNavigation(delta);
		Vector3 velocity = Velocity;

		// Add the gravity.
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

    //GlobalPosition = GlobalPosition.MoveToward(movement, speed);
    Vector3 moveDelta = GlobalPosition.MoveToward(movement, speed) - GlobalPosition;
		Velocity = moveDelta;
  }

}
