using System;
using Godot;

namespace Lloyd
{

    public static class MatrixExtensions
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
            position.X = matrix.M14;
            position.Y = matrix.M24;
            position.Z = matrix.M34;
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

            // Unity.Quaternion.LookRotation() Creates a rotation with the specified forward and upwards directions.
            Transform3D tfm = new Transform3D();
            tfm = tfm.LookingAt(forward, upwards);
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
            int i = 0;

        }

        //public static System.Numerics.Matrix4x4 HouseholderReflection(this System.Numerics.Matrix4x4 matrix4X4, Vector3 planeNormal)
        //{
        //	planeNormal.Normalized();
        //	Vector4 planeNormal4 = new Vector4(planeNormal.X, planeNormal.Y, planeNormal.Z, 0);
        //    System.Numerics.Matrix4x4 householderMatrix = System.Numerics.Matrix4x4.Identity.Minus(
        //		MultiplyVectorsTransposed(planeNormal4, planeNormal4).MutiplyByNumber(2));
        //	return householderMatrix * matrix4X4;
        //}

        //public static System.Numerics.Matrix4x4 MutiplyByNumber(this System.Numerics.Matrix4x4 matrix, float number)
        //{
        //	return new System.Numerics.Matrix4x4(
        //		matrix.M11 * number, matrix.M21 * number, matrix.M31 * number, matrix.M41 * number,
        //		matrix.M12 * number, matrix.M22 * number, matrix.M32 * number, matrix.M42 * number,
        //		matrix.M13 * number, matrix.M23 * number, matrix.M33 * number, matrix.M43 * number,
        //		matrix.M14 * number, matrix.M24 * number, matrix.M34 * number, matrix.M44 * number
        //	);
        //}

        //public static System.Numerics.Matrix4x4 DivideByNumber(this System.Numerics.Matrix4x4 matrix, float number)
        //{
        //	return new System.Numerics.Matrix4x4(
        //		matrix.M11 / number, matrix.M21 / number, matrix.M31 / number, matrix.M41 / number,
        //		matrix.M12 / number, matrix.M22 / number, matrix.M32 / number, matrix.M42 / number,
        //		matrix.M13 / number, matrix.M23 / number, matrix.M33 / number, matrix.M43 / number,
        //		matrix.M14 / number, matrix.M24 / number, matrix.M34 / number, matrix.M44 / number
        //	);
        //}

        //public static System.Numerics.Matrix4x4 Plus(this System.Numerics.Matrix4x4 matrix, System.Numerics.Matrix4x4 matrixToAdding)
        //{
        //	return new System.Numerics.Matrix4x4(
        //		matrix.M11 + matrixToAdding.M11, matrix.M21 + matrixToAdding.M21,
        //			matrix.M31 + matrixToAdding.M31, matrix.M41 + matrix.M41,
        //		matrix.M12 + matrixToAdding.M12, matrix.M22 + matrixToAdding.M22,
        //			matrix.M32 + matrixToAdding.M32, matrix.M42 + matrix.M42,
        //		matrix.M13 + matrixToAdding.M13, matrix.M23 + matrixToAdding.M23,
        //			matrix.M33 + matrixToAdding.M33, matrix.M43 + matrix.M43,
        //		matrix.M14 + matrixToAdding.M14, matrix.M24 + matrixToAdding.M24,
        //			matrix.M34 + matrixToAdding.M34, matrix.M44 + matrix.M44
        //	);
        //}

        //public static System.Numerics.Matrix4x4 Minus(this System.Numerics.Matrix4x4 matrix, System.Numerics.Matrix4x4 matrixToMinus)
        //{
        //	return new System.Numerics.Matrix4x4(
        //		matrix.M11 - matrixToMinus.M11, matrix.M21 - matrixToMinus.M21,
        //			matrix.M31 - matrixToMinus.M31, matrix.M41 - matrixToMinus.M41,
        //		matrix.M12 - matrixToMinus.M12, matrix.M22 - matrixToMinus.M22,
        //			matrix.M32 - matrixToMinus.M32, matrix.M42 - matrixToMinus.M42,
        //		matrix.M13 - matrixToMinus.M13, matrix.M23 - matrixToMinus.M23,
        //			matrix.M33 - matrixToMinus.M33, matrix.M43 - matrixToMinus.M43,
        //		matrix.M14 - matrixToMinus.M14, matrix.M24 - matrixToMinus.M24,
        //			matrix.M34 - matrixToMinus.M34, matrix.M44 - matrixToMinus.M44
        //	);
        //}
    }
}