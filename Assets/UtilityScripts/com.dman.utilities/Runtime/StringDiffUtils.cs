﻿using System.Collections.Generic;
using System.Text;

namespace Dman.Utilities
{
    public class StringDiffUtils
    {
        public static string StringEqualErrorMessage(string expected, string actual)
        {
            var errorMessage = new StringBuilder();
            errorMessage.AppendLine($"#### Expected ####\n{expected}\n#### Actual ####\n{actual}");
            var expectedStrLines = expected.Split('\n');
            var actualStrLines = actual.Split('\n');
            if (expectedStrLines.Length == actualStrLines.Length)
            {
                foreach (var (index, expectedLine, actualLine) in DiffLines(expectedStrLines, actualStrLines))
                {
                    var addlMessage = $"##{index}##\n- {expectedLine}\n+ {actualLine}";
                    errorMessage.AppendLine(addlMessage);
                }
            }
            return errorMessage.ToString();
        }
        
        private static IEnumerable<(int index, string expected, string actual)> DiffLines(string[] expected, string[] actual)
        {
            for (var i = 0; i < expected.Length; i++)
            {
                var expectedLine = expected[i].Trim();
                var actualLine = actual[i].Trim();
                if(expectedLine == actualLine) continue;
                yield return (i, expectedLine, actualLine);
            }
        }
    }
}