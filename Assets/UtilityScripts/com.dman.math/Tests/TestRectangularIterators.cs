using Dman.Math.RectangularIterators;
using NUnit.Framework;
using UnityEngine;
using static Dman.Math.RectangularIterators.RowColIterator;

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

        public void TestIterator(int[,] orderArray, IRectangularGroupIterator iterator)
        {
            var mapSize = new Vector2Int(orderArray.GetLength(1), orderArray.GetLength(0));
            var actualIteration = iterator.Iterate(mapSize);

            var actualMapOrder = new int[orderArray.GetLength(0), orderArray.GetLength(1)];

            for (int i = 0; i < orderArray.GetLength(0); i++)
            {
                for (int j = 0; j < orderArray.GetLength(1); j++)
                {
                    actualMapOrder[i, j] = -1;
                }
            }

            int index = 0;
            foreach (var coordinateGroup in actualIteration)
            {
                foreach (var coordinate in coordinateGroup)
                {
                    Assert.IsTrue(
                        coordinate.y < actualMapOrder.GetLength(0) && coordinate.y >= 0 &&
                        coordinate.x < actualMapOrder.GetLength(1) && coordinate.x >= 0,
                        $"Coordinate out of range of expected map size. coordinate: \n {coordinate}\nMap size: \n{mapSize}"
                        );
                    actualMapOrder[coordinate.y, coordinate.x] = index;
                }
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
        public void TestIterator(int[,] orderArray, IRectangularIterator iterator)
        {
            this.TestIterator(orderArray, new RectangularGroupAdapter(iterator));
            //var mapSize = new Vector2Int(orderArray.GetLength(1), orderArray.GetLength(0));
            //var actualIteration = iterator.Iterate(mapSize);

            //var actualMapOrder = new int[orderArray.GetLength(0), orderArray.GetLength(1)];
            //int index = 0;
            //foreach (var coordinate in actualIteration)
            //{
            //    Assert.IsTrue(
            //        coordinate.y < actualMapOrder.GetLength(0) && coordinate.y >= 0 &&
            //        coordinate.x < actualMapOrder.GetLength(1) && coordinate.x >= 0,
            //        $"Coordinate out of range of expected map size. coordinate: \n {coordinate}\nMap size: \n{mapSize}"
            //        );
            //    actualMapOrder[coordinate.y, coordinate.x] = index;
            //    index++;
            //}

            //for (int y = 0; y < orderArray.GetLength(0); y++)
            //{
            //    for (int x = 0; x < orderArray.GetLength(1); x++)
            //    {
            //        Assert.AreEqual(orderArray[y, x], actualMapOrder[y, x], $"Expected: \n{SerializeOrderArray(orderArray)} Actual:\n{SerializeOrderArray(actualMapOrder)}");
            //    }
            //}
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
            var actualIteration = new RowColIterator(RowColOrder.LeftRightBottomUp);
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
            var actualIteration = new RowColIterator(RowColOrder.TopDownRightLeft);
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
            var actualIteration = new RowColIterator(RowColOrder.RightLeftTopDown);
            TestIterator(expectedOrder, actualIteration);
        }

        [Test]
        public void TestIteratesSquareSpiral()
        {
            var expectedOrder =
                new int[5, 4]
                {
                    { 19, 18, 17, 16 },
                    {  6,  5,  4, 15 },
                    {  7,  0,  3, 14 },
                    {  8,  1,  2, 13 },
                    {  9, 10, 11, 12 },
                };
            var actualIteration = new SquareSpiralIterator(new Vector2(0.25f, 0.5f));
            TestIterator(expectedOrder, actualIteration);
        }
        [Test]
        public void TestIteratesBigSquareSpiral()
        {
            var expectedOrder =
                new int[7, 7]
                {
                    { 42, 41, 40, 39, 38, 37, 36 },
                    { 43, 20, 19, 18, 17, 16, 35 },
                    { 44, 21,  6,  5,  4, 15, 34 },
                    { 45, 22,  7,  0,  3, 14, 33 },
                    { 46, 23,  8,  1,  2, 13, 32 },
                    { 47, 24,  9, 10, 11, 12, 31 },
                    { 48, 25, 26, 27, 28, 29, 30 },
                };
            var actualIteration = new SquareSpiralIterator(new Vector2(0.5f, 0.5f));
            TestIterator(expectedOrder, actualIteration);
        }
        [Test]
        public void TestIteratesSquareSpiralFromCorner()
        {
            var expectedOrder =
                new int[5, 4]
                {
                    {  0,  3,  8, 15 },
                    {  1,  2,  7, 14 },
                    {  4,  5,  6, 13 },
                    {  9, 10, 11, 12 },
                    { 16, 17, 18, 19 },
                };
            var actualIteration = new SquareSpiralIterator(new Vector2(0, 0));
            TestIterator(expectedOrder, actualIteration);
        }
        [Test]
        public void TestIteratesSquareGroupFromCenter()
        {
            var expectedOrder =
                new int[6, 6]
                {
                    { 2, 2, 2, 2, 2, 2 },
                    { 2, 1, 1, 1, 1, 2 },
                    { 2, 1, 0, 0, 1, 2 },
                    { 2, 1, 0, 0, 1, 2 },
                    { 2, 1, 1, 1, 1, 2 },
                    { 2, 2, 2, 2, 2, 2 },
                };
            var actualIteration = new SquareGroupIterator(new Vector2(0.4f, 0.4f));
            TestIterator(expectedOrder, actualIteration);
        }
        [Test]
        public void TestIteratesSquareGroupFromCorner()
        {
            var expectedOrder =
                new int[6, 6]
                {
                    { 0, 0, 1, 2, 3, 4 },
                    { 0, 0, 1, 2, 3, 4 },
                    { 1, 1, 1, 2, 3, 4 },
                    { 2, 2, 2, 2, 3, 4 },
                    { 3, 3, 3, 3, 3, 4 },
                    { 4, 4, 4, 4, 4, 4 },
                };
            var actualIteration = new SquareGroupIterator(new Vector2(0f, 0f));
            TestIterator(expectedOrder, actualIteration);
        }
    }
}