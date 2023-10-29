using System.Collections;
using System.Collections.Generic;
using Godot;
using System;

namespace LDraw
{
	public static class TransformExtention
	{
		public static void ApplyLocalTRS(this Transform3D tr, System.Numerics.Matrix4x4 trs)
		{
			throw new NotImplementedException();

			//tr.localPosition = trs.ExtractPosition();
			//tr.localRotation = trs.ExtractRotation();
			//tr.localScale = trs.lossyScale;
		}
		
		public static System.Numerics.Matrix4x4 ExtractLocalTRS(this Transform3D tr)
		{
      throw new NotImplementedException();
      // Unity.Matrix4x4.TRS() Creates a translation, rotation and scaling matrix
      // return System.Numerics.Matrix4x4.TRS(tr.localPosition, tr.localRotation, tr.localScale);
		}

		public static void LocalReflect(this Transform3D tr, Vector3 planeNormal)
		{
      throw new NotImplementedException();
      //var trs = tr.ExtractLocalTRS();
      //var reflected = trs.HouseholderReflection(planeNormal);
      //tr.ApplyLocalTRS(reflected);
    }
	}
}