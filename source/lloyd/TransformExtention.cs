using System.Collections;
using System.Collections.Generic;
using Godot;
using System;

namespace Lloyd
{
    public static class TransformExtention
    {
        internal static System.Numerics.Vector3 FromGodotVector3(Godot.Vector3 vec)
        {
            return new System.Numerics.Vector3(vec.X, vec.Y, vec.Z);
        }

        public static Transform3D GetTransform(System.Numerics.Matrix4x4 trs)
        {
            Transform3D tr = new Transform3D();
            tr.Origin = MatrixExtensions.ExtractPosition(trs);
            tr.Basis = new Basis(MatrixExtensions.ExtractRotation(trs));
            tr.Basis = tr.Basis.Scaled(MatrixExtensions.ExtractScale(trs));
            return tr;
        }

    }
}