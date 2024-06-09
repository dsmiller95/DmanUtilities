using System.Collections.Generic;
using UnityEngine;

namespace Dman.Math
{
    public static class VectorUtilities
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="min">inclusive</param>
        /// <param name="max">exclusive</param>
        /// <returns></returns>
        public static IEnumerable<Vector3Int> IterateAllIn(Vector3Int min, Vector3Int max)
        {
            for (var x = min.x; x < max.x; x++)
            {
                for (var y = min.y; y < max.y; y++)
                {
                    for (var z = min.z; z < max.z; z++)
                    {
                        yield return new Vector3Int(x, y, z);
                    }
                }
            }
        }

        /// <summary>
        /// Iterate up from 0 (inclusive) to <paramref name="max"/> (exclusive)
        /// </summary>
        /// <param name="max">exclusive</param>
        /// <returns></returns>
        public static IEnumerable<Vector3Int> IterateAllIn(Vector3Int max)
        {
            return IterateAllIn(Vector3Int.zero, max);
        }

        
        public static Vector3Int[] AdjacentDirections = new[]
        {
            new Vector3Int(1, 0, 0),
            new Vector3Int(-1, 0, 0),
            new Vector3Int(0, 1, 0),
            new Vector3Int(0, -1, 0),
            new Vector3Int(0, 0, 1),
            new Vector3Int(0, 0, -1),
        };
        
        public static IEnumerable<Vector3Int> Neighbors(Vector3Int position)
        {
            foreach (var direction in AdjacentDirections)
            {
                yield return position + direction;
            }
        }
    }
}