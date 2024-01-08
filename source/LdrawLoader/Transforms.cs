using Godot;

namespace Ldraw
{
    public static class Transforms
    {
        private static readonly Transform3D m_ScaleToGameCoords;
        private static readonly Transform3D m_RotateToGameOrientation;
        private static readonly Transform3D m_RotateContainerTransforms;


        static Transforms()
        {
            m_RotateToGameOrientation = Transform3D.Identity;
            m_ScaleToGameCoords = Transform3D.Identity;
            m_RotateContainerTransforms = Transform3D.Identity;

            // Ldraw's coord space is the same orientation as Godot's, except that the Y axis is inverted.
            // The scale is 100:1, Ldr to Godot. This resolves to 1m in Godot = 4studs.
            // That makes a minifig about 1.5m tall, which works out very nicely.
            m_RotateToGameOrientation.Basis = m_RotateToGameOrientation.Basis.Rotated(Vector3.Forward, Mathf.Pi);
            m_ScaleToGameCoords.Basis = m_ScaleToGameCoords.Basis.Scaled(new Vector3(0.01f, 0.01f, 0.01f));

            Basis flipped = new();
            flipped.X = new Vector3(flipped.X.X, flipped.X.Y, - flipped.X.Z);
            flipped.Y = new Vector3(flipped.Y.X, flipped.Y.Y, flipped.Y.Z);
            flipped.Z = new Vector3(- flipped.Z.X, flipped.Z.Y, flipped.Z.Z);
            m_RotateContainerTransforms.Basis = flipped;
        }

        public static Transform3D GetScaleAndRotateToGameCoords()
        {
            return m_ScaleToGameCoords * m_RotateToGameOrientation;
        }

        public static Transform3D GetRotateToGameCoords()
        {
            return m_RotateToGameOrientation;
        }

        public static Transform3D GetCorrectedRotation(Transform3D tfm)
        {
            // Compensates for the fact that in LDraw space, Y+ is down vs
            // Godot, in which Y+ is up. This would cause rotation mirroring
            // in almost all cases.
            Basis flipped = tfm.Basis;
            flipped.X = new Vector3(flipped.X.X, flipped.X.Y, - flipped.X.Z);
            flipped.Y = new Vector3(flipped.Y.X, flipped.Y.Y, -flipped.Y.Z);
            flipped.Z = new Vector3(- flipped.Z.X, -flipped.Z.Y, flipped.Z.Z);
            tfm.Basis = flipped;  
            return tfm;
        }

        internal static System.Numerics.Vector3 FromGodotVector3(Godot.Vector3 vec)
        {
            return new System.Numerics.Vector3(vec.X, vec.Y, vec.Z);
        }

        public static Transform3D MakeGodotTransformFrom4x4(System.Numerics.Matrix4x4 matrix)
        {
            Transform3D tr = new Transform3D();
            tr.Origin = Matrix.ExtractPosition(matrix);
            tr.Basis = new Basis();
            tr.Basis.X = new Vector3(matrix.M11, matrix.M12, matrix.M13);
            tr.Basis.Y = new Vector3(matrix.M21, matrix.M22, matrix.M23);
            tr.Basis.Z = new Vector3(matrix.M31, matrix.M32, matrix.M33);
            return tr;
        }
    }
}