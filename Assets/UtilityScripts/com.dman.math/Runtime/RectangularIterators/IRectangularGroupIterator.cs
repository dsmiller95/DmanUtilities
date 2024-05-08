using System.Collections.Generic;
using UnityEngine;

namespace Dman.Math.RectangularIterators
{
    public interface IRectangularGroupIterator
    {
        public IEnumerable<ICollection<Vector2Int>> Iterate(Vector2Int size);
    }

    public class RectangularGroupAdapter : IRectangularGroupIterator
    {
        public IRectangularIterator Iterator { get; }
        public RectangularGroupAdapter(IRectangularIterator iterator)
        {
            Iterator = iterator;
        }

        public IEnumerable<ICollection<Vector2Int>> Iterate(Vector2Int size)
        {
            foreach (var coord in this.Iterator.Iterate(size))
            {
                yield return new Vector2Int[] { coord };
            }
        }
    }
}
