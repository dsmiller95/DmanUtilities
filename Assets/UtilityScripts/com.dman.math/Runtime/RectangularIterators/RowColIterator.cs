using System.Collections.Generic;
using UnityEngine;

namespace Dman.Math.RectangularIterators
{
    public class RowColIterator : IRectangularIterator
    {
        public RowColOrder ordering { get; }
        public RowColIterator(RowColOrder order)
        {
            this.ordering = order;
        }

        public enum RowColOrder
        {
            LeftRightBottomUp = 0b111,
            LeftRightTopDown = 0b011,
            RightLeftBottomUp = 0b101,
            RightLeftTopDown = 0b001,

            BottomUpLeftRight = 0b110,
            TopDownLeftRight = 0b010,
            BottomUpRightLeft = 0b100,
            TopDownRightLeft = 0b000,
        }

        /// <summary>
        /// iterates the rectangle in a row-column fashion
        /// </summary>
        /// <param name="size">The size of the iteration rectangle</param>
        /// <param name="ordering">the iteration order</param>
        /// <returns>An iterator over all values in <paramref name="size"/></returns>
        public IEnumerable<Vector2Int> Iterate(Vector2Int size)
        {
            var downUp = ((int)ordering & 0b100) != 0;
            var leftRight = ((int)ordering & 0b010) != 0;
            var widthFirst = ((int)ordering & 0b001) != 0;

            Vector2Int widthOffset = leftRight ? Vector2Int.right : Vector2Int.left;
            Vector2Int heightOffset = downUp ? Vector2Int.up : Vector2Int.down;

            var netDirectionality = widthOffset + heightOffset;
            netDirectionality = -1 * Vector2Int.Min(netDirectionality, Vector2Int.zero);
            var iterationOffset = netDirectionality * (size - Vector2Int.one);

            Vector2Int firstOrderOffset = widthFirst ? heightOffset : widthOffset;
            Vector2Int secondOrderOffset = widthFirst ? widthOffset : heightOffset;

            var firstOrderLimit = widthFirst ? size.y : size.x;
            var secondOrderLimit = widthFirst ? size.x : size.y;

            for (int i = 0; i < firstOrderLimit; i++)
            {
                for (int j = 0; j < secondOrderLimit; j++)
                {
                    yield return i * firstOrderOffset + j * secondOrderOffset + iterationOffset;
                }
            }
        }
    }

}
