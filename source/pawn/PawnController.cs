using GameManagerStates;
using Godot;
using System;


[GlobalClass, Icon("res://source/pawn/pawn.svg")]
public partial class PawnController : CharacterBody3D
{
	public const float m_speed = 5.0f;
	public const float m_jumpVelocity = 4.5f;
	public const float m_moveSpeed = 4.5f;
  private NavigationAgent3D m_navAgent;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

	public override void _Ready()
	{
		m_navAgent = GetNode<NavigationAgent3D>("navAgent");

		if (m_navAgent == null)
			GD.PrintErr("Missing nav agent!");
	}

	public override void _PhysicsProcess(double delta)
	{
		//DoNavigation(delta);
		Vector3 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
			velocity.Y -= gravity * (float)delta;

		Velocity = velocity;

    MoveAndSlide();
	}

	public void StartMovement(ActionPlan plan)
	{

	}

	private void DoNavigation(double delta) { throw  new NotImplementedException(); }
}
