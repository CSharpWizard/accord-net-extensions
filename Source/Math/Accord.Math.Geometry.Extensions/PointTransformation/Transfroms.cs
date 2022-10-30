﻿
namespace Accord.Math.Geometry
{
    public static class Transforms
    {
        #region Rotation

        public static float[,] RotationX(float angleRad)
        {
            var cos = (float)System.Math.Cos(angleRad);
            var sin = (float)System.Math.Sin(angleRad);

            return new float[,] 
            {
                {1, 0,   0},
                {0, cos, -sin},
                {0, sin, cos}
            };
        }

        public static float[,] RotationY(float angleRad)
        {
            var cos = (float)System.Math.Cos(angleRad);
            var sin = (float)System.Math.Sin(angleRad);

            return new float[,] 
            {
                {cos,  0, sin},
                {0,    1, 0},
                {-sin, 0, cos}
            };
        }

        public static float[,] RotationZ(float angleRad)
        {
            var cos = (float)System.Math.Cos(angleRad);
            var sin = (float)System.Math.Sin(angleRad);

            return new float[,] 
            {
                {cos, -sin, 0},
                {sin, cos,  0},
                {0,   0,    1}
            };
        }

        #endregion

        #region Translation

        public static float[,] Translation(float x, float y)
        {
            return new float[,] 
            {
                {1, 0, x},
                {0, 1, y},
                {0, 0, 1}
            };
        }

        #endregion

        #region Scale

        public static float[,] Scale(float x, float y, float z = 1)
        {
            return new float[,] 
            {
                {x, 0, 0},
                {0, y, 0},
                {0, 0, z}
            };
        }

        #endregion

        /// <summary>
        /// Multiplies transforms starting from the last one to the first one (stack).
        /// Transforms are given by priority.
        /// </summary>
        /// <param name="transforms">Transfrom matrices.</param>
        /// <returns>Combined transfrom matrix.</returns>
        public static float[,] Combine(params float[][,] transforms)
        {
            float[,] res = Matrix.Identity(3).ToSingle();

            for (int i = transforms.Length - 1; i >= 0; i--)
            {
                res = res.Multiply(transforms[i]);
            }

            return res;
        }

    }

}
