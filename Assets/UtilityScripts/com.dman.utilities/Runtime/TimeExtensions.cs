using System;

namespace MyUtilities
{
    public static class TimeExtensions
    {
        public static TimeSpan Divide(this TimeSpan timeSpan, double divisor)
        {
            return TimeSpan.FromTicks((long)(timeSpan.Ticks / divisor));
        }
    }

}
