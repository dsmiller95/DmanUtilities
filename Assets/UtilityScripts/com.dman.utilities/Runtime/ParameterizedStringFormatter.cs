using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Dman.Utilities
{
    public class ParameterizedStringFormatter
    {
        private string formatedString;
        public ParameterizedStringFormatter(string formatedString)
        {
            this.formatedString = formatedString;
        }

        public class FormatParameters
        {
            private Dictionary<string, string> parameters;
            public FormatParameters()
            {
                parameters = new Dictionary<string, string>();
            }

            public void Patch(FormatParameters patch)
            {
                foreach (var patchedKey in patch.parameters)
                {
                    if(patchedKey.Value == null)
                    {
                        parameters.Remove(patchedKey.Key);
                    }
                    else
                    {
                        parameters[patchedKey.Key] = patchedKey.Value;
                    }
                }
            }

            public void SetParameter(string parameter, string value)
            {
                parameters[parameter] = value;
            }

            internal string GetParameter(string parameter)
            {
                return parameters[parameter];
            }
            internal IEnumerable<string> GetParameterKeys()
            {
                return parameters.Keys;
            }
        }

        public string Format(FormatParameters parameters)
        {
            var sb = new StringBuilder(formatedString);
            foreach (var key in parameters.GetParameterKeys())
            {
                sb.Replace('{' + key + '}', parameters.GetParameter(key));
            }
            return sb.ToString();
        }


        private static readonly Regex formatMatcher = new Regex(@"\{.+?\}", RegexOptions.Compiled);
        public static bool IsFormattable(string formatString)
        {
            return formatMatcher.IsMatch(formatString);
        }
    }
}
