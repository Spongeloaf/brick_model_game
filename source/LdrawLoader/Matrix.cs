using System;
using Godot;

namespace Ldraw
{

    public static class Matrix
    {
        public static System.Numerics.Matrix4x4 MultiplyVectorsTransposed(Vector4 vector, Vector4 transposeVector)
        {

            float[] vectorPoints = new[] { vector.X, vector.Y, vector.Z, vector.W },
                transposedVectorPoints = new[]
                    {transposeVector.X, transposeVector.Y, transposeVector.Z, transposeVector.W};
            int matrixDimension = vectorPoints.Length;
            float[] values = new float[matrixDimension * matrixDimension];

            for (int i = 0; i < matrixDimension; i++)
            {
                for (int j = 0; j < matrixDimension; j++)
                {
                    values[i + j * matrixDimension] = vectorPoints[i] * transposedVectorPoints[j];
                }

            }

            return new System.Numerics.Matrix4x4(
               values[0], values[1], values[2], values[3],
               values[4], values[5], values[6], values[7],
               values[8], values[9], values[10], values[11],
               values[12], values[13], values[14], values[15]
           );
        }

        public static Vector3 ExtractPosition(in System.Numerics.Matrix4x4 matrix)
        {
            Vector3 position;
            position.X = matrix.M41;
            position.Y = matrix.M42;
            position.Z = matrix.M43;
            return position;
        }

        public static Quaternion ExtractRotation(in System.Numerics.Matrix4x4 matrix)
        {
            Vector3 forward;
            forward.X = matrix.M13;
            forward.Y = matrix.M23;
            forward.Z = matrix.M33;

            Vector3 upwards;
            upwards.X = matrix.M12;
            upwards.Y = matrix.M22;
            upwards.Z = matrix.M32;

            Transform3D tfm = new Transform3D();
            tfm = tfm.LookingAt(-forward, upwards);
            return new Quaternion(tfm.Basis);
        }

        public static Vector3 ExtractScale(in System.Numerics.Matrix4x4 matrix)
        {
            MatrixTests();

            Vector3 scale;
            scale.X = new Vector4(matrix.M11, matrix.M21, matrix.M31, matrix.M41).Length();
            scale.Y = new Vector4(matrix.M12, matrix.M22, matrix.M32, matrix.M42).Length();
            scale.Z = new Vector4(matrix.M13, matrix.M23, matrix.M33, matrix.M43).Length();
            return scale;
        }

        public static void MatrixTests()
        {
            System.Numerics.Matrix4x4 scale = System.Numerics.Matrix4x4.CreateScale(2.0f);
            System.Numerics.Matrix4x4 translate = System.Numerics.Matrix4x4.CreateTranslation(3.0f, 3.0f, 3.0f);
            System.Numerics.Matrix4x4 rotateX = System.Numerics.Matrix4x4.CreateRotationX(45.0f);
            System.Numerics.Matrix4x4 rotateY = System.Numerics.Matrix4x4.CreateRotationY(45.0f);
            System.Numerics.Matrix4x4 rotateZ = System.Numerics.Matrix4x4.CreateRotationZ(45.0f);
        }
    }
}