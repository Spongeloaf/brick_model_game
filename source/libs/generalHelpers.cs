using Godot;
using System;
using System.Runtime.InteropServices;

public class RaycastHit3D
{
  // Note: Member names match the dictionary counterparts. DON'T CHANGE THEM!
  public Vector3 position;         // point in world space for collision
  public Vector3 normal;            // normal in world space for collision
  public Object collider;          // Object collided or null (if unassociated)
  public int collider_id;          // Object it collided against
  //public RId rid;# RID it collide against
  public int shape;                // shape index of collider
  public VariantWrapper metadata;  // metadata of collider
}

// Put stuff here until you find a better place for it
public static class generalHelpers 
{

}
