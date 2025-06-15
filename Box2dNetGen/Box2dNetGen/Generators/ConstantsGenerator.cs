using System.Text;

namespace Box2dNetGen.Generators
{
    internal class ConstantsGenerator
    {
        public int GenerateConstants(List<ApiConstant> constants, StringBuilder sb)
        {
            var cnt = 0;
            foreach (var apiConstant in constants)
            {
                try
                {
                    sb.AppendLine();
                    CommentGenerator.AppendComment(sb, apiConstant.Comment, apiConstant.Type);
                    var code = $"public const {apiConstant.Type} {apiConstant.Identifier} = {apiConstant.Value};";
                    sb.AppendLine(code);
                    cnt++;
                }
                catch (NoGenException e)
                {
                    Console.WriteLine($"WARNING: skipping const '{apiConstant.Identifier}' because: {e.Message}");
                }
            }

            return cnt;
        }
    }
}
