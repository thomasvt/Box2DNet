using System.Text;
using System.Text.RegularExpressions;

namespace Box2dWrap
{
    internal class Program
    {
        // custom stuff: (because the generic conversion is naive, some special cases were easier to correct by just defining dedicated hooks with 
        // custom code to fix them.  These are here:

        private static readonly HashSet<string> _excludedTypes = new()
        {
            "b2Vec2", // replaced by System.Numerics.Vector2 using _structTypeReplacer
            "b2Timer", // needs work to translate correctly and .NET has timers of its own
            
            "b2DynamicTree", // needs work to translate because of unions and next-pointers
            "b2TreeNode",
            "b2DistanceCache",

            "b2DebugDraw" // can't translate the delegate-fields currenlty. Maybe translate and add it to the manually added C# code, DebugDraw rarely changes?
        };

        private static readonly Dictionary<string, Action<ApiStructField>> _structFieldModifiers = new()
        {
            // example from old version that couldn't handle fixed arrays yet: { "b2ShapeCastInput.points[b2_maxPolygonVertices]", ReplaceFixedVec2Array },
        };

        private static readonly Dictionary<string, string> _structTypeReplacer = new()
        {
            { "b2Vec2", "Vector2" }
        };

        /// <summary>
        /// A list of C structs for which we don't generate the initializer ctors (ctors with parameters to set the struct's fields)
        /// </summary>
        private static bool ShouldGenerateInitCtor(string structIdentifier)
        {
            if (structIdentifier.EndsWith("Def")) return false;
            if (structIdentifier.EndsWith("Id")) return false;
            if (structIdentifier.EndsWith("Output")) return false;
            return true;
        }

        private static string _extraUsings = "using System.Numerics;";

        //private static void ReplaceFixedVec2Array(ApiStructField field)
        //{
        //    // convert the fixed b2Vec2[] to deconstructed float[] 
        //    field.Comment.Add($"Original code: {field.Type} {field.Identifier}");
        //    field.Type = "float";
        //    field.Identifier = field.Identifier.Replace("b2_maxPolygonVertices", "B2Api.b2_maxPolygonVertices * 2");
        //}

        static async Task Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("BOX2DWRAP <box2d repo path> <C# output file path>");
                return;
            }

            var box2dFolder = args[0];
            var outputFile = args[1];
            if (Directory.Exists(Path.Combine(box2dFolder, "include\\box2d")) && File.Exists(Path.Combine(box2dFolder, "README.md")))
            {
                await BuildCsWrapperAsync(box2dFolder, outputFile);
                Console.WriteLine();
                Console.WriteLine($"C# Generated: \"{outputFile}\"");
            }
            else
            {
                Console.WriteLine("That folder does not seem to point to a Box2D v3 repository.");
            }
        }

        private static async Task BuildCsWrapperAsync(string box2dFolder, string csFilename)
        {
            var src = await ReadSourceAsync(Path.Combine(box2dFolder, "include\\box2d"));

            var constants = ExtractAllConstants(src).ToList();
            var structs = ExtractAllStructs(src).ToList();
            var delegates = ExtractAllDelegates(src).ToList();
            var functions = ExtractAllApiFunctions(src).ToList();
            var enums = ExtractAlLEnums(src).ToList();

            var generator = new CsGenerator(_extraUsings, _structTypeReplacer, ShouldGenerateInitCtor);

            var code = generator.GenerateCsCode(constants, structs, delegates, functions, enums, _excludedTypes);

            await File.WriteAllTextAsync(csFilename, code);
        }

        private static Regex apiConstantRegex = new("(?:(?:\\/{3}(?<comment>[^\\n]*)\\s*)*)\\s*#define\\s+(?<identifier>.*)\\s+(?<value>-?[0-9.]+)");

        private static IEnumerable<ApiConstant> ExtractAllConstants(string src)
        {
            foreach (Match match in apiConstantRegex.Matches(src))
            {
                var identifier = match.Groups["identifier"].Value;
                if (identifier.Contains('(')) continue; // skip weird constructions :)
                var value = match.Groups["value"].Value.Trim();
                var type = value.Contains('.') ? "float" : "int";
                if (type == "float" && !value.EndsWith("f")) value += "f";
                yield return new ApiConstant(identifier, value, type, GetMultiCaptures(match.Groups["comment"]));
            }
        }


        private static Regex apiDelegateRegex = new("(?:(?:\\/{3}(?<comment>[^\\n]*)\\s*)*)\\s*typedef\\s+(?<returnType>\\S+)\\s+(?<identifier>\\S+)\\s*\\((?<parameters>[^)]*)\\);");

        private static IEnumerable<ApiDelegate> ExtractAllDelegates(string src)
        {
            foreach (Match match in apiDelegateRegex.Matches(src))
            {
                var parameters = ExtractAllParameters(match.Groups["parameters"].Value);
                yield return new ApiDelegate(match.Groups["identifier"].Value, match.Groups["returnType"].Value, parameters, GetMultiCaptures(match.Groups["comment"]));
            }
        }

        private static Regex apiEnumRegex = new("typedef enum (?<identifier>\\S+)\\s*{(?<body>[\\s\\S]*?)}");

        private static IEnumerable<ApiEnum> ExtractAlLEnums(string src)
        {
            foreach (Match match in apiEnumRegex.Matches(src))
            {
                var body = match.Groups["body"].Value.Trim();
                var fields = ExtractAllEnumFields(body).ToList();
                yield return new ApiEnum(match.Groups["identifier"].Value, fields);
            }
        }

        private static Regex apiEnumFieldsRegex = new("(?:(?:\\/{3}(?<comment>[^\\n]*)\\s*)*)\\s*(?<identifier>\\w+)(?:\\s*=\\s*(?<value>\\w+))?,");

        private static IEnumerable<ApiEnumField> ExtractAllEnumFields(string src)
        {
            foreach (Match match in apiEnumFieldsRegex.Matches(src))
            {
                var value = match.Groups["value"].Success ? match.Groups["value"].Value : null;
                yield return new ApiEnumField(match.Groups["identifier"].Value, value, GetMultiCaptures(match.Groups["comment"]));
            }
        }


        private static Regex apiStructRegex = new("(?:(?:\\/{3}(?<comment>[^\\n]*)\\s*)*)\\s*typedef struct (?<identifier>\\S+)\\s*{(?<body>[\\s\\S]*?)\\n\\s*}");

        private static IEnumerable<ApiStruct> ExtractAllStructs(string src)
        {
            foreach (Match match in apiStructRegex.Matches(src))
            {
                var body = match.Groups["body"].Value;
                var identifier = match.Groups["identifier"].Value.Trim();

                if (_excludedTypes.Contains(identifier))
                    continue;

                if (body.Contains("#if"))
                {
                    Console.WriteLine($"WARNING: struct {match.Groups["identifier"].Value} is skipped because its body contains precompiler directives which are not supported atm.");
                    continue;
                }

                var fields = ExtractAllStructFields(identifier, body).ToList();
                yield return new ApiStruct(identifier, fields, GetMultiCaptures(match.Groups["comment"]));
            }
        }

        private static Regex apiStructFieldsRegex = new("(?:(?:\\/{2,3}(?<comment>[^\\n]*)\\s*)*)\\s*(?<type>\\w[\\w\\s\\*]+)\\s+(?<identifier>[^;]+);\\s*(\\/{3}<(?<comment>[^\\n]*))?", RegexOptions.Multiline);
        private static Regex fixedArrayRegex = new("(?<identifier>[^\\[]+)\\s*\\[\\s*(?<length>[^\\]]+)\\s*\\]");

        private static IEnumerable<ApiStructField> ExtractAllStructFields(string structIdentifier, string src)
        {
            foreach (Match match in apiStructFieldsRegex.Matches(src))
            {
                var identifier = match.Groups["identifier"].Value.Trim();
                var inlineArrayMatch = fixedArrayRegex.Match(identifier);
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
                        GetMultiCaptures(match.Groups["comment"]));
                    if (_structFieldModifiers.TryGetValue($"{structIdentifier}.{subIdentifier}", out var modifier))
                        modifier(field);

                    yield return field;
                }
            }
        }

        private static List<string> GetMultiCaptures(Group matchGroup)
        {
            return matchGroup.Captures.Select(c => c.Value.Trim()).ToList();
        }

        private static Regex apiFunctionRegex = new("(?:(?:\\/{3}(?<comment>[^\\n]*)\\s*)*)\\s*B2_API\\s+(?<returnType>\\S+)\\s+(?<identifier>\\S+)\\(\\s*(?<params>[^)]*)\\);");

        private static IEnumerable<ApiFunction> ExtractAllApiFunctions(string src)
        {
            foreach (Match match in apiFunctionRegex.Matches(src))
            {
                if (!match.Success) continue;
                var parameters = ExtractAllParameters(match.Groups["params"].Value);
                yield return new ApiFunction(match.Groups["identifier"].Value, match.Groups["returnType"].Value, parameters, GetMultiCaptures(match.Groups["comment"]));
            }
        }

        private static List<ApiParameter> ExtractAllParameters(string src)
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




        private static async Task<string> ReadSourceAsync(string folder)
        {
            var sb = new StringBuilder();
            foreach (var file in Directory.GetFiles(folder, "*.h"))
            {
                sb.AppendLine(await File.ReadAllTextAsync(file));
            }
            return sb.ToString();
        }
    }
}
