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
            return Vector3Int.Min(a, b);
        }
        public static Vector3Int MaxComponents(this Vector3Int a, Vector3Int b)
        {
            return Vector3Int.Max(a, b);
        }
        public static int MaxAxis(this Vector3Int vect)
        {
            return Mathf.Max(vect.x, vect.y, vect.z);
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

        public static Vector3Int MaskLargest(this Vector3Int vect)
        {
            var abs = vect.AbsComponents();
            var max = abs.MaxAxis();
            if (max == abs.x)
            {
                return new Vector3Int((int)Mathf.Sign(vect.x), 0, 0);
            }
            if (max == abs.y)
            {
                return new Vector3Int(0, (int)Mathf.Sign(vect.y), 0);
            }
            return new Vector3Int(0, 0, (int)Mathf.Sign(vect.z));
        }
    }
}
