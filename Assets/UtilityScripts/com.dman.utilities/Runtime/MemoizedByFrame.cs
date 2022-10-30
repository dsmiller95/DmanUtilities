using System;
using UnityEngine;

namespace Dman.Utilities
{
    public class MemoizedByFrame<T>
    {
        private int lastRefreshFrame = 0;
        private T lastValue;
        private Func<T> evaluate;

        public T Value
        {
            get
            {
                if (lastRefreshFrame != Time.frameCount)
                {
                    lastValue = evaluate();
                    lastRefreshFrame = Time.frameCount;
                }
                return lastValue;
            }
        }

        public MemoizedByFrame(Func<T> evaluate)
        {
            this.evaluate = evaluate;
        }
    }
}
