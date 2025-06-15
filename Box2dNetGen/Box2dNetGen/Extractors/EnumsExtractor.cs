using System.Text.RegularExpressions;

namespace Box2dNetGen.Extractors
{
    internal class EnumsExtractor
    {
        private readonly static Regex ApiEnumRegex = new("typedef enum (?<identifier>\\S+)\\s*{(?<body>[\\s\\S]*?)}");

        public static IEnumerable<ApiEnum> ExtractAlLEnums(string src)
        {
            foreach (Match match in ApiEnumRegex.Matches(src))
            {
                var body = match.Groups["body"].Value.Trim();
                var fields = ExtractAllEnumFields(body).ToList();
                yield return new ApiEnum(match.Groups["identifier"].Value, fields);
            }
        }

        private readonly static Regex ApiEnumFieldsRegex = new("(?:(?:\\/{3}(?<comment>[^\\n]*)\\s*)*)\\s*(?<identifier>\\w+)(?:\\s*=\\s*(?<value>\\w+))?,");

        private static IEnumerable<ApiEnumField> ExtractAllEnumFields(string src)
        {
            foreach (Match match in ApiEnumFieldsRegex.Matches(src))
            {
                var value = match.Groups["value"].Success ? match.Groups["value"].Value : null;
                yield return new ApiEnumField(match.Groups["identifier"].Value, value, ExtractorUtils.GetMultiCaptures(match.Groups["comment"]));
            }
        }
    }
}
