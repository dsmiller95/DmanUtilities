using UnityEngine;

namespace Dman.Utilities
{
    public static class OrientationExtensions
    {
        public static (Vector3 position, Quaternion rotation, Vector3 scale) DecomposeTRS(this Matrix4x4 matrix)
        {
            // Extract new local position
            Vector3 position = matrix.GetColumn(3);

            // Extract new local rotation
            Quaternion rotation = Quaternion.LookRotation(
                matrix.GetColumn(2),
                matrix.GetColumn(1)
            );

            // Extract new local scale
            Vector3 scale = new Vector3(
                matrix.GetColumn(0).magnitude,
                matrix.GetColumn(1).magnitude,
                matrix.GetColumn(2).magnitude
            );

            return (position, rotation, scale);
        }
    }
}
