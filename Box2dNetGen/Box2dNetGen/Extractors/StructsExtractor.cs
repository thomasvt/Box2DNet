using System.Text.RegularExpressions;

namespace Box2dNetGen.Extractors
{
    internal class StructsExtractor(HashSet<string> excludedTypes, Dictionary<string, Action<ApiStructField>> structFieldModifiers)
    {
        private readonly static Regex ApiStructRegex = new("(?:(?:\\/{3}(?<comment>[^\\n]*)\\s*)*)\\s*typedef struct (?<identifier>\\S+)\\s*{(?<body>[\\s\\S]*?)\\n\\s*}");

        public IEnumerable<ApiStruct> ExtractAllStructs(string src)
        {
            foreach (Match match in ApiStructRegex.Matches(src))
            {
                var body = match.Groups["body"].Value;
                var identifier = match.Groups["identifier"].Value.Trim();

                if (excludedTypes.Contains(identifier))
                    continue;

                if (body.Contains("#if"))
                {
                    Console.WriteLine($"WARNING: struct {match.Groups["identifier"].Value} is skipped because its body contains precompiler directives which are not supported atm.");
                    continue;
                }

                var fields = ExtractAllStructFields(identifier, body).ToList();
                yield return new ApiStruct(identifier, fields, ExtractorUtils.GetMultiCaptures(match.Groups["comment"]));
            }
        }

        private readonly static Regex ApiStructFieldsRegex = new("(?:(?:\\/{2,3}(?<comment>[^\\n]*)\\s*)*)\\s*(?<type>\\w[\\w\\s\\*]+)\\s+(?<identifier>[^;]+);\\s*(\\/{3}<(?<comment>[^\\n]*))?", RegexOptions.Multiline);
        private readonly static Regex FixedArrayRegex = new("(?<identifier>[^\\[]+)\\s*\\[\\s*(?<length>[^\\]]+)\\s*\\]");

        private IEnumerable<ApiStructField> ExtractAllStructFields(string structIdentifier, string src)
        {
            foreach (Match match in ApiStructFieldsRegex.Matches(src))
            {
                var identifier = match.Groups["identifier"].Value.Trim();
                var inlineArrayMatch = FixedArrayRegex.Match(identifier);
                var isFixedArray = false;
                var arrayLength = string.Empty;
                if (inlineArrayMatch.Success) // it's a C inline array (fixed-sized array that sits inline in the struct's memory layout)
                {
                    isFixedArray = true;
                    identifier = inlineArrayMatch.Groups["identifier"].Value;
                    arrayLength = inlineArrayMatch.Groups["length"].Value;
                }

                var type = match.Groups["type"].Value;

                foreach (var subIdentifier in identifier.Split(',').Select(i => i.Trim())) // -> 'int a, b' are actually 2 fields with the same type, so split the identifier and generate all fields
                {
                    var field = new ApiStructField(subIdentifier, type, isFixedArray, arrayLength,
                        ExtractorUtils.GetMultiCaptures(match.Groups["comment"]));
                    if (structFieldModifiers.TryGetValue($"{structIdentifier}.{subIdentifier}", out var modifier))
                        modifier(field);

                    yield return field;
                }
            }
        }
    }
}
