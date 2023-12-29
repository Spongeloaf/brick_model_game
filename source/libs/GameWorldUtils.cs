using Godot;
using System;
using Godot.Collections;
using System.Linq;
using GameManagerStates;
using BrickModelGame.source.CodeResources;

public struct RaycastHit3D
{
    // Note: Member names match the dictionary counterparts. DON'T CHANGE THEM!
    public Vector3 position;          // point in world space for collision
    public Vector3 normal;            // normal in world space for collision
    public GodotObject collider;           // Object collided or null (if unassociated)
    public int collider_id;           // Object it collided against
    public Rid rid;                   // RID it collide against
    public int shape;                 // shape index of collider
    public Variant metadata;   // metadata of collider
    public bool DidCollide() { return position != Vector3.Inf; }
}


// Put stuff here until you find a better place for it
public static class GameWorldUtils
{
    public static RaycastHit3D ConstructRaycastHit3D(Dictionary raycastDict)
    {
        RaycastHit3D hit = new RaycastHit3D();
        hit.position = Vector3.Inf;

        // We return on a count of less than 6 because metadata is not always present
        if (raycastDict == null || raycastDict.Count < 6)
            return hit;

        if (raycastDict.ContainsKey("position"))
            hit.position = (Vector3)raycastDict["position"];
        else
            hit.position = Vector3.Inf; // Standard practice for unknown Vector3 values

        if (raycastDict.ContainsKey("normal"))
            hit.normal = (Vector3)raycastDict["normal"];

        if (raycastDict.ContainsKey("collider"))
            hit.collider = raycastDict["collider"].As<GodotObject>();

        if (raycastDict.ContainsKey("collider_id"))
            hit.collider_id = (int)raycastDict["collider_id"];

        if (raycastDict.ContainsKey("rid"))
            hit.rid = (Rid)raycastDict["rid"];

        if (raycastDict.ContainsKey("shape"))
            hit.shape = (int)raycastDict["shape"];

        if (raycastDict.ContainsKey("metadata"))
            hit.metadata = (Variant)raycastDict["metadata"];

        return hit;
    }

    public static RaycastHit3D DoRaycastInDirection(World3D world, Vector3 from, Vector3 direction,
                                         float length = Mathf.Inf, Rid[] excludes = null, uint collision_mask = 4294967295)
    {
        direction = direction.Normalized();
        Vector3 to = from + direction * length;
        Array<Rid> excludes_array = new Array<Rid>();
        foreach (Rid id in excludes)
            excludes_array.Add(id);
        var query = PhysicsRayQueryParameters3D.Create(from, to, collision_mask, excludes_array);
        query.CollideWithAreas = true;
        Dictionary result = world.DirectSpaceState.IntersectRay(query);
        RaycastHit3D hit = ConstructRaycastHit3D(result);

        // Always return the point limited to 'length' argument
        if (hit.position == Vector3.Inf)
            hit.position = to;

        return hit;
    }

    public static RaycastHit3D DoRaycastPointToPoint(World3D world, Vector3 from, Vector3 to,
                                         float limit = Mathf.Inf, Array<Rid> excludes = null, uint collision_mask = 4294967295)
    {
        var query = PhysicsRayQueryParameters3D.Create(from, to, collision_mask, excludes);
        Dictionary result = world.DirectSpaceState.IntersectRay(query);
        return ConstructRaycastHit3D(result);
    }

    public static ulong CalculateMoveTimeInMsec(in ActionPlan plan)
    {
        if (plan == null || plan.actor == null)
            return 0;

        if (plan.actor.m_statCard == null || plan.path.Length == 0)
            return 0;

        ulong travelTimeInSeconds = (ulong)(Math.GetPathLength(plan.path) / plan.actor.m_statCard.moveSpeed);
        return travelTimeInSeconds * 1000;
    }

    // Given a parent and child nodes, the child ir rotated, relative to the parent, to face the target.
    // Rotation is restricted to the supplied axis in parent-local space.
    //
    // Example: Imagine a tank turret. If you call Node3d.LookAt() on the turret, it will rotate on 
    // all three axes to face the target, which would look stupid. However, use this function and pass
    // in Vector3.Up, and it will rotate the turret on it's local y axis, which will look correct even
    // if the tank is parked on an incline, or flipped on its roof.
    // 
    // Call this function again on a node that elevates the tank's gun, and pass in Vector3.Right, and
    // now the gun will elevate to face the target as best it can. This should produce a realistic looking
    // result, as though a human hamd manually aimed the turret to compensate for the incline.
    //
    // !!WARNING!! This function does not currently support angular restrictions. It assumes the child
    // node is free to rotate to any point on its axis, which fine for turret bases but not so much for
    // gun elevation nodes.
    // TODO: Fix that
    //
    // How do I fix that? I think we could pass in two values: A positive and negative angle limit. Then
    // comapre those to the forward vector of the parent frame. This would allow for asymetric limits!
    public static void LookAtOnAxis(in Vector3 globalTarget, in Vector3 localAxis, in Node3D parentObject, Node3D childToAim)
    {
        Vector3 localTarget = parentObject.ToLocal(globalTarget);
        Transform3D localUpTfm = parentObject.Transform.TranslatedLocal(localAxis);
        Vector3 planeNormal = localUpTfm.Origin.Normalized();
        Plane rotationPlane = new Plane(planeNormal, parentObject.Position);
        Vector3 projectedPoint = rotationPlane.Project(localTarget);

        if (CustomProjectSettings.GetDebugTurretAiming())
        {
            DebugDraw3D.DrawLine(parentObject.GlobalPosition, parentObject.ToGlobal(rotationPlane.GetCenter() + rotationPlane.Normal * 5), Colors.Green);
            DebugDraw3D.DrawSphere(parentObject.ToGlobal(localTarget), 0.25f, Colors.Red);
            DebugDraw3D.DrawSphere(parentObject.ToGlobal(projectedPoint), 0.25f, Colors.Green);
        }

        float angleRadians = Mathf.Atan2(projectedPoint.X, projectedPoint.Z);
        OmniLogger.Info("Angle: " + angleRadians);
        childToAim.Rotation = Vector3.Zero;
        childToAim.RotateObjectLocal(localAxis, angleRadians);
    }
} // GameWorldUtils
