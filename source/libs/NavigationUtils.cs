using Godot;
using System;
using Godot.Collections;
using System.Linq;

// This might be better off as a struct.....
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
}

// Put stuff here until you find a better place for it
public static class NavigationUtils 
{
  public static RaycastHit3D ConstructRaycastHit3D(Dictionary raycastDict)
  {
    RaycastHit3D hit = new RaycastHit3D();

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

  public static RaycastHit3D DoRaycast(World3D world, Vector3 from, Vector3 direction, 
                                       float limit = Mathf.Inf, Array<Rid> excludes = null, uint collision_mask = 4294967295)
  {
    direction = direction.Normalized();
    Vector3 to = from + direction * limit;
    var query = PhysicsRayQueryParameters3D.Create(from, to, collision_mask, excludes);
    Dictionary result = world.DirectSpaceState.IntersectRay(query);
    return ConstructRaycastHit3D(result);
  }


} // generalHelpers
