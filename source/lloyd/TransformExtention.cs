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

        public static Transform3D GetTransform(System.Numerics.Matrix4x4 matrix)
        {
            Transform3D tr = new Transform3D();
            tr.Origin = MatrixExtensions.ExtractPosition(matrix);
            tr.Basis = new Basis();
            //tr.Basis.X = new Vector3(matrix.M11, matrix.M21, matrix.M31);
            //tr.Basis.Y = new Vector3(matrix.M12, matrix.M22, matrix.M32);
            //tr.Basis.Z = new Vector3(matrix.M13, matrix.M23, matrix.M33);
            tr.Basis.X = new Vector3(matrix.M11, matrix.M12, matrix.M13);
            tr.Basis.Y = new Vector3(matrix.M21, matrix.M22, matrix.M23);
            tr.Basis.Z = new Vector3(matrix.M31, matrix.M32, matrix.M33);

            return tr;
        }

    }
}