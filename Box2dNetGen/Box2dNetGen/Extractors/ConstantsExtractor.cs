using System.Text.RegularExpressions;

namespace Box2dNetGen.Extractors
{
    internal class ConstantsExtractor
    {
        private readonly static Regex ApiConstantRegex = new("(?:(?:\\/{3}(?<comment>[^\\n]*)\\s*)*)\\s*#define\\s+(?<identifier>\\S+)\\s+\\{?(?<value>-?[0-9.]+)\\}?");

        /// <summary>
        /// Extracts all `#define` constants that are useful for Box2dNet.
        /// </summary>
        public static IEnumerable<ApiConstant> ExtractAllPrecompilerDefines(string src)
        {
            Console.WriteLine("Constants:");
            foreach (Match match in ApiConstantRegex.Matches(src))
            {
                var identifier = match.Groups["identifier"].Value;
                if (identifier.Contains('(')) continue; // skip #define's of functions
                var value = match.Groups["value"].Value.Trim();
                var type = value.Contains('.') ? "float" : "int";
                if (type == "float" && !value.EndsWith("f")) value += "f";
                Console.WriteLine(identifier);
                yield return new ApiConstant(identifier, value, type, ExtractorUtils.GetMultiCaptures(match.Groups["comment"]));
            }
            Console.WriteLine("end of constants");
        }
    }
}
