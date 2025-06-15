using System.Text;
using Box2dNetGen.Extractors;

namespace Box2dNetGen
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

            "b2DebugDraw" // no support for translating the delegate-fields. I wrote a translation manually in `B2Api.DebugDraw.cs` because this code rarely changes
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
                Console.WriteLine("BOX2DNETGEN <box2d repo path> <C# output file path>");
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

            var constants = ConstantsExtractor.ExtractAllPrecompilerDefines(src).ToList();
            var structs = new StructsExtractor(_excludedTypes, _structFieldModifiers).ExtractAllStructs(src).ToList();
            var delegates = DelegatesExtractor.ExtractAllDelegates(src).ToList();
            var functions = ApiFunctionsExtractor.ExtractAllApiFunctions(src).ToList();
            var enums = EnumsExtractor.ExtractAlLEnums(src).ToList();

            var generator = new CsGenerator(_extraUsings, _structTypeReplacer, ShouldGenerateInitCtor);

            var code = generator.GenerateCsCode(constants, structs, delegates, functions, enums, _excludedTypes);

            await File.WriteAllTextAsync(csFilename, code);
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
