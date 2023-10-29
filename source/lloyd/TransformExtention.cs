using System.Collections;
using System.Collections.Generic;
using Godot;
using System;

namespace LDraw
{
	public static class TransformExtention
	{
    internal static System.Numerics.Vector3 FromGodotVector3(Godot.Vector3 vec)
    {
      return new System.Numerics.Vector3(vec.X, vec.Y, vec.Z);
    }

		public static void ApplyLocalTRS(this Transform3D tr, System.Numerics.Matrix4x4 trs)
		{
      tr.Origin = trs.ExtractPosition();
      tr.Basis = new Basis(trs.ExtractRotation());
      tr.Basis = tr.Basis.Scaled(trs.ExtractScale());
    }

    public static System.Numerics.Matrix4x4 ExtractLocalTRS(this Transform3D tr)
		{
      // GODOT PORT: I don't know if this is the right way to do this.

      // Original code:
      // Unity.Matrix4x4.TRS() Creates a translation, rotation and scaling matrix
      // return System.Numerics.Matrix4x4.TRS(tr.localPosition, tr.localRotation, tr.localScale);

      Godot.Quaternion srcQuat = tr.Basis.GetRotationQuaternion();
      System.Numerics.Quaternion dstQuat = new System.Numerics.Quaternion(srcQuat.X, srcQuat.Y, srcQuat.Z, srcQuat.W);

      System.Numerics.Matrix4x4 translation = System.Numerics.Matrix4x4.CreateTranslation(FromGodotVector3(tr.Origin));
      System.Numerics.Matrix4x4 rotation = System.Numerics.Matrix4x4.CreateFromQuaternion(dstQuat);
      System.Numerics.Matrix4x4 scale = System.Numerics.Matrix4x4.CreateScale(FromGodotVector3(tr.Basis.Scale));
      
      return translation + rotation + scale;
    }

		public static void LocalReflect(this Transform3D tr, Vector3 planeNormal)
		{
      System.Numerics.Matrix4x4 trs = tr.ExtractLocalTRS();
      System.Numerics.Matrix4x4 reflected = trs.HouseholderReflection(planeNormal);
      tr.ApplyLocalTRS(reflected);
    }
	}
}