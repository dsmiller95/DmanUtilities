using UnityEngine;

namespace Dman.Utilities
{
    public static class VectorExtensions
    {
        public static Vector2 InverseScale(this Vector2 source, Vector2 scale)
        {
            return new Vector2(source.x / scale.x, source.y / scale.y);
        }
    }
}
