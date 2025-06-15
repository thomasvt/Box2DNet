using System.Text.RegularExpressions;

namespace Box2dNetGen.Extractors
{
    internal class ApiFunctionsExtractor
    {
        private readonly static Regex ApiFunctionRegex = new("(?:(?:\\/{3}(?<comment>[^\\n]*)\\s*)*)\\s*B2_API\\s+(?<returnType>\\S+)\\s+(?<identifier>\\S+)\\(\\s*(?<params>[^)]*)\\);");

        public static IEnumerable<ApiFunction> ExtractAllApiFunctions(string src)
        {
            foreach (Match match in ApiFunctionRegex.Matches(src))
            {
                if (!match.Success) continue;
                var parameters = ParameterListingExtractor.ExtractAllParameters(match.Groups["params"].Value);
                yield return new ApiFunction(match.Groups["identifier"].Value, match.Groups["returnType"].Value, parameters, ExtractorUtils.GetMultiCaptures(match.Groups["comment"]));
            }
        }
    }
}
