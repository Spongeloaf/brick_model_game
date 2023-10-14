using GameManagerStates;
using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

[GlobalClass, Icon("res://source/pawn/pawn.svg")]
public partial class PawnController : RigidBody3D
{
  [Export] private float m_speed = 10f;
  [Export] private float m_snapToGroundDistance = 5.0f;
  [Export] public StatCard m_statCard;

  private const float m_angleTolerance = 0.0872665f; // equals roughly 5 degrees
  private NavigationAgent3D m_navAgent;
  private CollisionShape3D m_collisionShape;
  private List<Node3D> m_targetPoints;

  // Get the gravity from the project settings to be synced with RigidBody nodes.
  public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

  public override void _Ready()
  {
    FreezeMode = FreezeModeEnum.Kinematic;
    Freeze = false;
    AddConstantCentralForce(new Vector3(0f, -gravity, 0f));

    m_navAgent = GetNode<NavigationAgent3D>("navAgent");
    if (m_navAgent == null)
      GD.PrintErr("Missing nav agent!");

    m_navAgent.AvoidanceEnabled = true;

    m_collisionShape = GetNode<CollisionShape3D>("collider");
    if (m_collisionShape == null)
      GD.PrintErr("Failed to find collider!");

    m_targetPoints = FindTargetPointsInChildren();
    m_statCard = GetNode<StatCard>("statCard");
    if (m_statCard == null)
    {
      GD.PrintErr("Pawn stat card not found! Creating default.....");
      m_statCard = new StatCard();
      m_statCard.moveDistance = 20;
      m_statCard.armor = 4;
      m_statCard.skillDie = 6;
      m_statCard.skillBonus = 0;
    }
  }

  public override void _PhysicsProcess(double delta)
  {
    if (m_navAgent == null)
      return;

    if (m_navAgent.IsNavigationFinished())
    {
      FinishNavigation();
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

    Vector3 facing = GlobalTransform.Basis.Y;
    float angle = facing.AngleTo(Vector3.Up);

    // Flip the pawn upright in case its lying down somehow
    if (angle > m_angleTolerance)
    {
      Transform3D newTfm = GlobalTransform;
      newTfm.Basis = Basis.Identity;
      GlobalTransform = newTfm;
    }

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

    RaycastHit3D hit = NavigationUtils.DoRaycastInDirection(
      GetWorld3D(), newPosition, Vector3.Down, m_snapToGroundDistance, exclusions);

    if (hit.position == Vector3.Inf)
      return;

    float difference = GlobalPosition.Y - hit.position.Y;
    string str = string.Format("y diff: {0}", difference);
    GD.Print(str);

    newPosition.Y = hit.position.Y;
    GlobalPosition = newPosition;
  }

  public bool IsNavigating()
  {
    if (m_navAgent == null)
      return false;

    return !m_navAgent.IsNavigationFinished();
  }

  public NavigationAgent3D GetNavigationAgent3D() { return m_navAgent; }

  // Target points are used to calculate aiming penalties to other pawns when targeting this pawn.
  // Returns the pawns global position if no targetting points found. This function is not in the
  // PawnUtils library only because it caches the points.
  public List<Node3D> FindTargetPointsInChildren()
  { 
    List<Node3D> result = new List<Node3D>();
    result.Add(this);
    Node3D pointsParent = GetNode<Node3D>("targetPoints");
    if (pointsParent == null)
      return result;

    Array<Node> pointNodes = pointsParent.GetChildren();
    if (pointNodes.Count < 1)
      return result;

    // We only return our self positon in case we can't find the real array,
    // so we have SOMETHING to target. But once we find real target points, we
    // want to replace them.
    result.Clear();
    foreach (Node node in pointNodes)
    {
      try
      {
        result.Add((Node3D)node);
      }
      catch
      {
        GD.PrintErr("PawnController.GetTargetPoints found non-3D nodes as children of the targetting node!");
      };
    }

    return result;
  }

  // Return an array of globabl postions that represent a rough outline of the
  // pawn for aiming purposes. When other pawn target this one, they can do a
  // line-of-sight check to each point to see how much of the pawn is obstructed.
  public Vector3[] GetTargetPoints()
  {
    List<Vector3> result = new List<Vector3>();
    foreach(Node3D node in m_targetPoints)
      result.Add(((Node3D)node).GlobalPosition);

    return result.ToArray();
  }

  public StatCard GetStatCard() { return m_statCard; }
}
