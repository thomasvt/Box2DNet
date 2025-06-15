using System.Text;

namespace Box2dNetGen.Generators
{
    internal class DelegatesGenerator(TypeMapper typeMapper, GeneratorUtils utils)
    {
        public int GenerateDelegates(List<ApiDelegate> delegates, StringBuilder sb)
        {
            var cnt = 0;
            foreach (var apiDelegate in delegates)
            {
                try
                {
                    sb.AppendLine();
                    CommentGenerator.AppendComment(sb, apiDelegate.Comment, apiDelegate.ReturnType, apiDelegate.Parameters.ToDictionary(p => p.Identifier, p =>
                        $"(Original C type: {p.Type})"));
                    var parameters = utils.GenerateParameterList(apiDelegate.Parameters, true, true, out _);
                    sb.AppendLine("  [UnmanagedFunctionPointer(CallingConvention.Cdecl)]");
                    sb.AppendLine(
                        $"  public delegate {typeMapper.MapType(apiDelegate.ReturnType, false, CodeDirection.ClrToNative, true, out _)} {apiDelegate.Identifier}({parameters});");
                    cnt++;
                }
                catch (NoGenException e)
                {
                    Console.WriteLine($"WARNING: skipping delegate '{apiDelegate.Identifier}' because: {e.Message}");
                    ;
                }
            }

            return cnt;
        }
    }
}
