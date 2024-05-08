using System.Collections.Generic;
using UnityEngine;

namespace Dman.Math.RectangularIterators
{
    public interface IRectangularIterator
    {
        public IEnumerable<Vector2Int> Iterate(Vector2Int size);
    }
}
