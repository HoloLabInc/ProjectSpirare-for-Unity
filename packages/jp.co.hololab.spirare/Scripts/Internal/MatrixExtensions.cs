using System;
using UnityEngine;

namespace HoloLab.Spirare
{
    internal static class MatrixExtensions
    {
        /// <summary>
        /// Extracts Position from a Matrix4x4
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static Vector3 ExtractPosition(this Matrix4x4 matrix)
        {
            return matrix.GetColumn(3);
        }

        /// <summary>
        /// Extracts Rotation from a Matrix4x4
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static Quaternion ExtractRotation(this Matrix4x4 matrix)
        {
            return Quaternion.LookRotation(matrix.GetColumn(2), matrix.GetColumn(1));
        }
    }
}
