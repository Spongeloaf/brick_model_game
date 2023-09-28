using Godot;
using System;

public partial class pawn : CharacterBody3D
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
		Vector3 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
			velocity.Y -= gravity * (float)delta;

		Velocity = velocity;
		DoNavigation(delta);

    MoveAndSlide();
	}

	private void DoNavigation(double delta)
	{
		if (m_navAgent == null)
			return;

		if (m_navAgent.IsNavigationFinished())
			return;

		// TODO next: Figure out how to make this move on command!
		Math.LerpAlongPath(m_navAgent.GetCurrentNavigationPath(), 0.0f, 2f);
	}
}
