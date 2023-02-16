using System.Collections.Generic;
using UnityEngine;

namespace Dman.Utilities
{
    public interface IRectangularIterator
    {
        public IEnumerable<Vector2Int> Iterate(Vector2Int size);
    }
}
