using System;
using System.Collections.Generic;
using System.Linq;

namespace Dman.Utilities
{
    public static class RandomExtensions
    {
        public static int PickWeighted(this Random rand, float[] weights)
        {
            var totalWeight = weights.Sum();
            var randomPoint = (float)rand.NextDouble() * totalWeight;
        
            for (var i = 0; i < weights.Length; i++)
            {
                if (randomPoint < weights[i])
                {
                    return i;
                }
                randomPoint -= weights[i];
            }
            return weights.Length - 1;
        }

        public static T PickAnyEnumWeighted<T>(this Random rand, float[] weights)
        {
            if(weights.Length != Enum.GetValues(typeof(T)).Length)
            {
                throw new ArgumentException("weights.Length must be equal to the number of values in the enum");
            }
            var index = rand.PickWeighted(weights);
            return (T)Enum.GetValues(typeof(T)).GetValue(index);
        }
    
        public static T PickAnyEnum<T>(this Random rand) where T : Enum
        {
            var values = Enum.GetValues(typeof(T));
            return (T)values.GetValue(rand.Next(0, values.Length));
        }
        public static T PickEnumBetween<T>(this Random rand, T minEnumValExclusive, T maxEnumValExclusive) where T : Enum
        {
            var minIntVal = (int)(object)(minEnumValExclusive) + 1;
            var maxIntVal = (int)(object)(maxEnumValExclusive);
            if(minIntVal > maxIntVal)
            {
                throw new ArgumentException("minEnumValExclusive must be less than maxEnumValExclusive");
            }
            var enumVal = rand.Next(minIntVal, maxIntVal);
            return (T)Enum.ToObject(typeof(T), enumVal);
        }
        public static T PickEnumUpTo<T>(this Random rand, T maxEnumValExclusive) where T : Enum
        {
            var minIntVal = 0;  
            var maxIntVal = (int)(object)(maxEnumValExclusive);
            var enumVal = rand.Next(minIntVal, maxIntVal);
            return (T)Enum.ToObject(typeof(T), enumVal);
        }
    
        public static double NextNormal(this Random rand, float mean, float stdDev)
        {
            // https://stackoverflow.com/a/218600
            double u1 = 1.0-rand.NextDouble(); //uniform(0,1] random doubles
            double u2 = 1.0-rand.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                                   Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
            double randNormal =
                mean + stdDev * randStdNormal; //random normal(mean,stdDev^2)
            return randNormal;
        }
        
        public static T PickRandom<T>(this Random rand, T[] array)
        {
            return array[rand.Next(0, array.Length)];
        }
        public static T PickRandom<T>(this Random rand, IList<T> array)
        {
            return array[rand.Next(0, array.Count)];
        }
        
    }
}