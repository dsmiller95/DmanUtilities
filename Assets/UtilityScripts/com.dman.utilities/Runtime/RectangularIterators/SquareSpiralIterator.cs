using System.Collections.Generic;
using UnityEngine;

namespace Dman.Utilities
{
    public class SquareSpiralIterator : IRectangularIterator
    {
        public Vector2 relativeOrigin { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="relativeOrigin"></param>
        public SquareSpiralIterator(Vector2 relativeOrigin)
        {
            this.relativeOrigin = relativeOrigin;
        }

        /// <summary>
        /// iterates the rectangle in a row-column fashion
        /// </summary>
        /// <param name="size">The size of the iteration rectangle</param>
        /// <param name="ordering">the iteration order</param>
        /// <returns>An iterator over all values in <paramref name="size"/></returns>
        public IEnumerable<Vector2Int> Iterate(Vector2Int size)
        {
            var origin = new Vector2Int((int)(relativeOrigin.x * size.x), (int)(relativeOrigin.y * size.y));

            foreach (var spiralItem in UnfilteredSpiral(Mathf.Max(size.x, size.y) * 2 + 1))
            {
                var nextItem = spiralItem + origin;
                if (nextItem.x < 0 || nextItem.x >= size.x ||
                   nextItem.y < 0 || nextItem.y >= size.y)
                {
                    continue;
                }
                yield return nextItem;
            }
        }

        private Vector2Int[] spiralDirections = new[]
        {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left
        };

        private Vector2Int GetDirection(int directionIndex)
        {
            return spiralDirections[(directionIndex + spiralDirections.Length) % spiralDirections.Length];
        }
        /// <summary>
        /// generates a list of coordinates in a square spiral originating at 0,0 . generating up to <paramref name="maxSpiralSteps"/> spiral rings
        /// </summary>
        /// <param name="maxSpiralSteps"></param>
        /// <returns></returns>
        private IEnumerable<Vector2Int> UnfilteredSpiral(int maxSpiralSteps = 1000)
        {
            var currentPoint = Vector2Int.zero;
            var direction = 0;

            for (int sideLen = 1; sideLen < maxSpiralSteps; sideLen++)
            {
                for(int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < sideLen; j++)
                    {
                        yield return currentPoint;
                        currentPoint += GetDirection(direction);
                    }
                    direction++;
                }
            }
        }
    }

}
