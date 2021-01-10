using System.Collections.Generic;
using UnityEngine;

namespace Dman.Utilities
{
    public static class ArrayExtensions
    {
        public static T[][] Pivot<T>(this T[][] source)
        {
            if (source.Length <= 0 || source[0].Length <= 0)
            {
                throw new System.Exception("invalid pivot");
            }
            var result = new T[source[0].Length][];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new T[source.Length];
                for (int j = 0; j < result[i].Length; j++)
                {
                    result[i][j] = source[j][i];
                }
            }
            return result;
        }

        /// <summary>
        /// return a list of indexes of length <paramref name="numberOfIndexes"/> varying from 0 to <paramref name="sizeOfIndexedSpace"/>
        ///     such that no number is repeated twice
        /// </summary>
        /// <param name="numberOfIndexes"></param>
        /// <param name="sizeOfIndexedSpace"></param>
        /// <returns></returns>
        public static int[] SelectIndexSources(int numberOfIndexes, int sizeOfIndexedSpace)
        {
            if (numberOfIndexes > sizeOfIndexedSpace)
            {
                throw new System.Exception("must have at least as many parents as chromosome copies");
            }
            var selectedParents = new HashSet<int>();
            var resultSelectedParentsPerDuplicate = new int[numberOfIndexes];
            for (int i = 0; i < resultSelectedParentsPerDuplicate.Length; i++)
            {
                int nextSelectedParent;
                do
                {
                    nextSelectedParent = Random.Range(0, sizeOfIndexedSpace);
                } while (selectedParents.Contains(nextSelectedParent));
                selectedParents.Add(nextSelectedParent);
                resultSelectedParentsPerDuplicate[i] = nextSelectedParent;
            }
            return resultSelectedParentsPerDuplicate;
        }
    }
}
