using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dman.Math.RectangularIterators
{
    public class SquareGroupIterator : IRectangularGroupIterator
    {
        public Vector2 relativeOrigin { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="relativeOrigin"></param>
        public SquareGroupIterator(Vector2 relativeOrigin)
        {
            this.relativeOrigin = relativeOrigin;
        }

        /// <summary>
        /// iterates the rectangle in a row-column fashion
        /// </summary>
        /// <param name="size">The size of the iteration rectangle</param>
        /// <param name="ordering">the iteration order</param>
        /// <returns>An iterator over all values in <paramref name="size"/></returns>
        public IEnumerable<ICollection<Vector2Int>> Iterate(Vector2Int size)
        {
            var origin = new Vector2Int((int)(relativeOrigin.x * size.x), (int)(relativeOrigin.y * size.y));

            var maxSize = Mathf.Max(size.x, size.y);

            for (int i = 0; i < maxSize; i++)
            {
                var square = GetSquareElements(i)
                    .Select(x => x + origin)
                    .Where(nextItem => 
                       nextItem.x >= 0 && nextItem.x < size.x &&
                       nextItem.y >= 0 && nextItem.y < size.y)
                    .ToList();
                if (!square.Any())
                {
                    yield break;
                }
                yield return square;
            }
        }

        private IEnumerable<Vector2Int> GetSquareElements(int radius)
        {
            var sideLen = radius * 2 + 1;

            var direction = 0;
            var currentPoint = (sideLen / 2) *
                (
                    GetDirection(direction - 1) +
                    GetDirection(direction - 2)
                );


            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < sideLen; j++)
                {
                    yield return currentPoint;
                    currentPoint += GetDirection(direction);
                }
                direction++;
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
    }

}
