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

                    var parameters = utils.GenerateMappedParameterList(apiFunction.Parameters);
                    var returnSymbol = typeMapper.MapType(apiFunction.ReturnType, false, CodeDirection.ClrToNative,
                        true, out _, out _);

                    var unsafeModifier = parameters.Any(x => x.SubParameters.Any(y => y.IsUnsafe))
                        ? "unsafe "
                        : string.Empty;

                    sbI.AppendLine();
                    CommentGenerator.AppendComment(sbI, apiFunction.Comment, apiFunction.ReturnType,
                        apiFunction.Parameters.ToDictionary(p => p.Identifier, p => $"(Original C type: {p.Type})"));
                    sbI.AppendLine("[DllImport(Box2DLibrary, CallingConvention = CallingConvention.Cdecl)]");
                    sbI.AppendLine(
                        $"public static extern {unsafeModifier}{returnSymbol} {apiFunction.Identifier}({parameters.ToCsv()});");

                    // generate overloads
                    for (int i = 0; i < parameters.Count; i++)
                    {
                        var param = parameters[i];
                        if (param.AltParameters.Count == 0) continue;


                        MainParameter[] previousElements = parameters.Slice(0, i).ToArray();
                        MainParameter[] nextElements = parameters.Slice(i + 1, parameters.Count - (i + 1)).ToArray();


                        foreach (var altParameter in param.AltParameters)
                        {
                            var isUnsafeAlt = altParameter.ParamAndArgs.All(x => x.Parameter.IsUnsafe) ||
                                              previousElements.Any(x => x.SubParameters.Any(y => y.IsUnsafe)) ||
                                              nextElements.Any(x => x.SubParameters.Any(y => y.IsUnsafe));

                            var commentDict = previousElements.Concat(nextElements)
                                .SelectMany(x => x.SubParameters)
                                .ToDictionary(p => p.Identifier,
                                    p => $"(Original C type: {p.Type})");

                            foreach (var paramAndArg in altParameter.ParamAndArgs)
                            {
                                if (paramAndArg.RepresentsApiParameters.Count == 1)
                                {
                                    commentDict.Add(paramAndArg.Parameter.Identifier,
                                        paramAndArg.RepresentsApiParameters.Select(x => $"(Original C type: {x.Type})")
                                            .First());
                                }
                                else
                                {
                                    var commentStr = "Original parameters: " + string.Join("; ",
                                        paramAndArg.RepresentsApiParameters.Select(x =>
                                            $"\"{x.Identifier}\" (original C type: {x.Type})"));
                                    commentDict.Add(paramAndArg.Parameter.Identifier, commentStr);
                                }
                            }

                            sbI.AppendLine();
                            CommentGenerator.AppendOverloadComment(sbI, apiFunction.Comment, apiFunction.ReturnType,
                                commentDict);

                            if (altParameter.IsAlsoExtern)
                            {
                                sbI.AppendLine(
                                    "[DllImport(Box2DLibrary, CallingConvention = CallingConvention.Cdecl)]");
                            }

                            var externModifier = altParameter.IsAlsoExtern ? "extern " : string.Empty;
                            var externEnd = altParameter.IsAlsoExtern ? ";" : string.Empty;
                            var unsafeModifierAlt = isUnsafeAlt ? "unsafe " : string.Empty;


                            var overloadParameterSelect = previousElements.SelectMany(x => x.SubParameters)
                                .Concat(altParameter.ParamAndArgs.Select(x => x.Parameter))
                                .Concat(nextElements.SelectMany(x => x.SubParameters));

                            sbI.AppendLine(
                                $"public static {externModifier}{unsafeModifierAlt}{returnSymbol} {apiFunction.Identifier}({overloadParameterSelect.ToCsv()}){externEnd}");

                            if (!altParameter.IsAlsoExtern)
                            {
                                sbI.AppendLine("{");
                                int indentLevel = 1;

                                foreach (var paramAndArg in altParameter.ParamAndArgs)
                                {
                                    bool useWrap = paramAndArg.CallWrap.HasValue;
                                    if (useWrap)
                                    {
                                        AppendIndent(sbI, indentLevel);
                                        sbI.AppendLine(paramAndArg.GetPreCallStr());
                                    }

                                    if (useWrap && paramAndArg.CallWrap!.Value.IncrementScope)
                                    {
                                        AppendIndent(sbI, indentLevel);
                                        sbI.AppendLine("{");
                                        indentLevel++;
                                    }
                                }

                                var argSelect = previousElements.SelectMany(x => x.SubParameters)
                                    .Select(x => x.Identifier)
                                    .Concat([altParameter.ToArgString()])
                                    .Concat(nextElements.SelectMany(x => x.SubParameters).Select(x => x.Identifier));

                                AppendIndent(sbI, indentLevel);
                                sbI.AppendLine(
                                    $"{(returnSymbol == "void" ? "" : "return ")}{apiFunction.Identifier}({argSelect.ToCsv()});");
                                altParameter.ToArgString();

                                foreach (var paramAndArg in altParameter.ParamAndArgs)
                                {
                                    if (paramAndArg.CallWrap?.IncrementScope ?? false)
                                    {
                                        indentLevel--;
                                        AppendIndent(sbI, indentLevel);
                                        sbI.AppendLine("}");
                                    }
                                }

                                sbI.AppendLine("}");
                            }
                        }
                    }

                    sb.Append(sbI); // commit
                    cnt++;
                }
                catch (NoGenException e)
                {
                    Console.WriteLine($"WARNING: skipping function '{apiFunction.Identifier}' because: {e.Message}");
                }
            }

            return cnt;
        }

        private static void AppendIndent(StringBuilder sb, int indentLevel)
        {
            const string indent = "    ";
            for (int i = 0; i < indentLevel; i++)
            {
                sb.Append(indent);
            }
        }
    }
}