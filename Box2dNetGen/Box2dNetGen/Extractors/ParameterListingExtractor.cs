using System.Text.RegularExpressions;

namespace Box2dNetGen.Extractors
{
    internal class ParameterListingExtractor
    {
        /// <summary>
        /// Gets all parameters from a parameter listing.
        /// </summary>
        public static List<ApiParameter> ExtractAllParameters(string src)
        {
            var list = new List<ApiParameter>();
            if (src.Trim() == "void") return list;
            foreach (var part in src.Split(','))
            {
                list.Add(ParseSingleParameter(part.Trim()));
            }

            return list;
        }

        static Regex SingleParameterRegex = new("(?<type>.+)\\s+(?<identifier>\\S+)");

        private static ApiParameter ParseSingleParameter(string src)
        {
            var match = SingleParameterRegex.Match(src);
            if (!match.Success)
            {
                throw new Exception($"Invalid function parameter: '{src}'.");
            }

            return new ApiParameter(match.Groups["identifier"].Value, match.Groups["type"].Value);
        }
    }
}
