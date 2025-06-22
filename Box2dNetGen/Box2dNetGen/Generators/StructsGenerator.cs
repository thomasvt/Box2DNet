using System.Diagnostics;
using System.Text;

namespace Box2dNetGen.Generators
{
    internal class StructsGenerator(TypeMapper typeMapper, Func<string, bool> shouldGenerateInitCtor)
    {
        private record ClrStructField(string Identifier, string ClrType);

        public int GenerateStructs(List<ApiStruct> structs, List<ApiConstant> constants, StringBuilder sb)
        {
            var cnt = 0;
            foreach (var apiStruct in structs)
            {
                try
                {
                    var sbI = new StringBuilder();
                    sbI.AppendLine();
                    CommentGenerator.AppendComment(sbI, apiStruct.Comment, null);
                    sbI.AppendLine("[StructLayout(LayoutKind.Sequential)]");
                    sbI.AppendLine($"public partial struct {apiStruct.Identifier}");
                    sbI.AppendLine("{");

                    var clrFields = new List<ClrStructField>(apiStruct.Fields.Count);
                    var structHasNoArrayFields = true;

                    foreach (var field in apiStruct.Fields)
                    {
                        sbI.AppendLine();
                        CommentGenerator.AppendComment(sbI, field.Comment, field.Type);
                        var clrType = typeMapper.MapType(field.Type, false, CodeDirection.NativeToClr, true, out _);
                        if (field.IsFixedArray)
                        {
                            structHasNoArrayFields = false;
                            GenerateInlineArrayField(constants, field, sbI, clrType);
                        }
                        else
                        {
                            GenerateSimpleField(field, sbI, clrType);
                            clrFields.Add(new ClrStructField(field.Identifier, clrType));
                        }
                    }

                    if (structHasNoArrayFields)
                        GenerateConvenienceCtor(sbI, apiStruct.Identifier, clrFields);

                    sbI.AppendLine("}");

                    sb.Append(sbI.ToString()); // commit

                    cnt++;
                }
                catch (NoGenException e)
                {
                    Console.WriteLine($"WARNING: skipping struct '{apiStruct.Identifier}' because: {e.Message}");
                }
            }

            return cnt;
        }
        
        private static void GenerateSimpleField(ApiStructField field, StringBuilder sbI, string clrType)
        {
            if (field.Type == "bool")
                sbI.AppendLine("  [MarshalAs(UnmanagedType.U1)]"); // else .NET marshals as 32 bit, very fun to narrow down on that one.

            var identifier = GetSafeIdentifier(field.Identifier);

            sbI.AppendLine($"  public {clrType} {identifier};");
        }

        /// <summary>
        /// Generates code for a field that is an inline array in C, which does not exist in C#. SO we generate an unrolled version with repeated fields and an indexed method to mimic the array-access.
        /// </summary>
        private static void GenerateInlineArrayField(List<ApiConstant> constants, ApiStructField field, StringBuilder sbI, string clrType)
        {
            var arrayLength = GetOrResolveArrayLength(constants, field);

            // PROBLEM: commented out below code because this Marshal ByValArray thing doesn't seem to work. Get memory access issues at runtime ...
            // var arrayLength = cte == null ? field.ArrayLength : "B2Api." + cte.Identifier;
            // sbI.AppendLine($"  [MarshalAs(UnmanagedType.ByValArray, SizeConst = {arrayLength})]");

            // SOLUTION: so we just repeat the fields to mimic the inline array, and add a helper method to get them by index:

            var identifier = GetSafeIdentifier(field.Identifier);

            var switchCases = new List<string>();
            ;
            for (var i = 0; i < arrayLength; i++)
            {
                sbI.AppendLine($"  public {clrType} {identifier}{i};");
                switchCases.Add($"      {i} => {identifier}{i},\r\n");
            }
            sbI.AppendLine($"  /// <summary>.NET helper to get the inline {identifier} by index. </summary>");
            sbI.AppendLine($"  public {clrType} {identifier}(int idx)\r\n  {{\r\n    return idx switch\r\n    {{");
            sbI.Append(string.Join("", switchCases));
            sbI.AppendLine($"      _ => throw new ArgumentOutOfRangeException(nameof(idx), \"There are only {arrayLength} {identifier}.\")\r\n    }};\r\n  }}");
        }

        private static string GetSafeIdentifier(string identifier)
        {
            if (identifier.Equals("base", StringComparison.InvariantCultureIgnoreCase))
                return "@base";
            return identifier;
        }

        /// <summary>
        /// Returns field.ArrayLength as integer, or if it's a Constant, returns the value of that constant.
        /// </summary>
        private static int GetOrResolveArrayLength(List<ApiConstant> constants, ApiStructField field)
        {
            var cte = constants.Find(c => c.Identifier == field.ArrayLength);
            var arrayLength = int.Parse(cte == null ? field.ArrayLength : cte.Value);
            return arrayLength;
        }

        /// <summary>
        /// Generates a convenience constructor (for .NET) that accepts all fields as parameters. Only does this for simple structs.
        /// </summary>
        private void GenerateConvenienceCtor(StringBuilder sb, string structIdentifier, IReadOnlyCollection<ClrStructField> fields)
        {
            if (!shouldGenerateInitCtor(structIdentifier))
                return;

            sb.AppendLine();
            sb.Append($"  public {structIdentifier}(");
            sb.Append(string.Join(", ", fields.Select(f => $"in {f.ClrType} {GetSafeIdentifier(f.Identifier)}")));
            sb.AppendLine(")");
            sb.AppendLine("  {");
            sb.AppendLine(string.Join(Environment.NewLine, fields.Select(f => $"    this.{GetSafeIdentifier(f.Identifier)} = {GetSafeIdentifier(f.Identifier)};")));
            sb.AppendLine("  }");
            sb.AppendLine();
        }
    }
}
