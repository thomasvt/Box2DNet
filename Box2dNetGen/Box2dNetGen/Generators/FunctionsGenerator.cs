using System.Text;

namespace Box2dNetGen.Generators
{
    internal class FunctionsGenerator(TypeMapper typeMapper)
    {
        public int GenerateFunctions(List<ApiFunction> functions, StringBuilder sb)
        {
            var utils = new GeneratorUtils(typeMapper);

            var cnt = 0;
            foreach (var apiFunction in functions)
            {
                try
                {
                    var sbI = new StringBuilder();
                    var parameters = utils.GenerateParameterList(apiFunction.Parameters, true, true, out var containsDelegateParameters);
                    sbI.AppendLine();
                    CommentGenerator.AppendComment(sbI, apiFunction.Comment, apiFunction.ReturnType, apiFunction.Parameters.ToDictionary(p => p.Identifier, p => $"(Original C type: {p.Type})"));
                    sbI.AppendLine($"[DllImport(Box2DLibrary, CallingConvention = CallingConvention.Cdecl)]");
                    sbI.AppendLine(
                        $"public static extern {typeMapper.MapType(apiFunction.ReturnType, false, CodeDirection.ClrToNative, true, out _)} {apiFunction.Identifier}({parameters});");

                    if (containsDelegateParameters)
                    {
                        // also generate C# overload that accepts the strongly typed delegate instead of IntPtr.
                        sbI.AppendLine();
                        parameters = utils.GenerateParameterList(apiFunction.Parameters, false, false, out _);
                        var arguments = utils.GenerateArgumentList(apiFunction.Parameters);
                        CommentGenerator.AppendComment(sbI, apiFunction.Comment, apiFunction.ReturnType, apiFunction.Parameters.ToDictionary(p => p.Identifier, p => $"(Original C type: {p.Type})"));
                        sbI.AppendLine(
                            $"public static {typeMapper.MapType(apiFunction.ReturnType, false, CodeDirection.ClrToNative, true, out _)} {apiFunction.Identifier}({parameters})");
                        var @return = apiFunction.ReturnType != "void" ? "return " : "";
                        sbI.AppendLine(
                            $"{{\r\n    {@return}{apiFunction.Identifier}({arguments});\r\n}}");

                    }

                    sb.Append(sbI.ToString()); // commit
                    cnt++;
                }
                catch (NoGenException e)
                {
                    Console.WriteLine($"WARNING: skipping function '{apiFunction.Identifier}' because: {e.Message}");
                    ;
                }
            }

            return cnt;
        }
    }
}
