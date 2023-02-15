using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Dman.Utilities.RectangularIterator;

namespace Dman.Utilities
{
    public class TestRectangularIterator
    {
        public string SerializeOrderArray(int[,] orderArray)
        {
            var output = "";
            for (int y = 0; y < orderArray.GetLength(0); y++)
            {
                for (int x = 0; x < orderArray.GetLength(1); x++)
                {
                    output += orderArray[y, x] + ", ";
                }
                output += "\n";
            }
            return output;
        }

        public void TestIterator(int[,] orderArray, IEnumerable<Vector2Int> generatedIterator)
        {
            var actualIteration = generatedIterator.ToList();

            var actualMapOrder = new int[orderArray.GetLength(0), orderArray.GetLength(1)];
            int index = 0;
            foreach (var coordinate in actualIteration)
            {
                Assert.IsTrue(
                    coordinate.y < actualMapOrder.GetLength(0) && coordinate.y >= 0 &&
                    coordinate.x < actualMapOrder.GetLength(1) && coordinate.x >= 0,
                    $"Coordinate out of range of expected map size. coordinate: \n {coordinate}\nMap size: \n{new Vector2Int(actualMapOrder.GetLength(1), actualMapOrder.GetLength(0))}"
                    );
                actualMapOrder[coordinate.y, coordinate.x] = index;
                index++;
            }

            for (int y = 0; y < orderArray.GetLength(0); y++)
            {
                for (int x = 0; x < orderArray.GetLength(1); x++)
                {
                    Assert.AreEqual(orderArray[y, x], actualMapOrder[y, x], $"Expected: \n{SerializeOrderArray(orderArray)} Actual:\n{SerializeOrderArray(actualMapOrder)}");
                }
            }
        }


        [Test]
        public void TestIteratesLeftRightBottomUp()
        {
            var expectedOrder =
                new int[3, 4]
                {
                    { 0, 1,  2,  3 },
                    { 4, 5,  6,  7 },
                    { 8, 9, 10, 11 },
                };
            var actualIteration = RectangularIterator.IterateRowColumn(new Vector2Int(4, 3), RowColOrder.LeftRightBottomUp);
            TestIterator(expectedOrder, actualIteration);
        }

        [Test]
        public void TestIteratesTopDownRightLeft()
        {
            var expectedOrder =
                new int[3, 4]
                {
                    { 11, 8, 5, 2 },
                    { 10, 7, 4, 1 },
                    { 9 , 6, 3, 0 },
                };
            var actualIteration = RectangularIterator.IterateRowColumn(new Vector2Int(4, 3), RowColOrder.TopDownRightLeft);
            TestIterator(expectedOrder, actualIteration);
        }

        [Test]
        public void TestIteratesRightLeftTopDown()
        {
            var expectedOrder =
                new int[5, 4]
                {
                    { 19, 18, 17, 16 },
                    { 15, 14, 13, 12 },
                    { 11, 10,  9,  8 },
                    {  7,  6,  5,  4 },
                    {  3,  2,  1,  0 },
                };
            var actualIteration = RectangularIterator.IterateRowColumn(new Vector2Int(4, 5), RowColOrder.RightLeftTopDown);
            TestIterator(expectedOrder, actualIteration);
        }
    }
}