using System.Text.RegularExpressions;

namespace Box2dNetGen.Extractors
{
    internal class DelegatesExtractor
    {
        private readonly static Regex ApiDelegateRegex = new("(?:(?:\\/{3}(?<comment>[^\\n]*)\\s*)*)\\s*typedef\\s+(?<returnType>\\S+)\\s+(?<identifier>\\S+)\\s*\\((?<parameters>[^)]*)\\);");

        public static IEnumerable<ApiDelegate> ExtractAllDelegates(string src)
        {
            foreach (Match match in ApiDelegateRegex.Matches(src))
            {
                var parameters = ParameterListingExtractor.ExtractAllParameters(match.Groups["parameters"].Value);
                yield return new ApiDelegate(match.Groups["identifier"].Value, match.Groups["returnType"].Value, parameters, ExtractorUtils.GetMultiCaptures(match.Groups["comment"]));
            }
        }
    }
}
