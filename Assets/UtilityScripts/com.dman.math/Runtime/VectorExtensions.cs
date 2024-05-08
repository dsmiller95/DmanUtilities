using UnityEngine;

namespace Dman.Math
{
    public static class VectorExtensions
    {
        public static Vector2 InverseScale(this Vector2 source, Vector2 scale)
        {
            return new Vector2(source.x / scale.x, source.y / scale.y);
        }

        public static int DistanceManhattan(this Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }
        
        public static Vector3Int MinComponents(this Vector3Int a, Vector3Int b)
        {
            return new Vector3Int(
                Mathf.Min(a.x, b.x),
                Mathf.Min(a.y, b.y),
                Mathf.Min(a.z, b.z)
            );
        }
        public static Vector3Int MaxComponents(this Vector3Int a, Vector3Int b)
        {
            return new Vector3Int(
                Mathf.Max(a.x, b.x),
                Mathf.Max(a.y, b.y),
                Mathf.Max(a.z, b.z)
            );
        }
    
        public static Vector3Int AbsComponents(this Vector3Int a)
        {
            return new Vector3Int(
                Mathf.Abs(a.x),
                Mathf.Abs(a.y),
                Mathf.Abs(a.z)
            );
        }

        public static int GetNonzeroAxisCount(this Vector3Int vector)
        {
            return (vector.x != 0 ? 1 : 0) + (vector.y != 0 ? 1 : 0) + (vector.z != 0 ? 1 : 0);
        }

    }
}
