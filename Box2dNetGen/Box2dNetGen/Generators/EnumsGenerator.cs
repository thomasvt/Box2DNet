using System.Text;

namespace Box2dNetGen.Generators
{
    internal class EnumsGenerator
    {
        public int GenerateEnums(List<ApiEnum> enums, StringBuilder sb)
        {
            var cnt = 0;
            foreach (var apiEnum in enums)
            {
                sb.AppendLine();
                sb.AppendLine($"public enum {apiEnum.Identifier}");
                sb.AppendLine("{");
                foreach (var field in apiEnum.Fields)
                {
                    sb.AppendLine();
                    CommentGenerator.AppendComment(sb, field.Comment, null);

                    var valuePart = field.Value == null ? "" : $" = {field.Value}";
                    sb.AppendLine($"  {field.Identifier}{valuePart},");
                }

                sb.AppendLine("}");

                cnt++;
            }

            return cnt;
        }
    }
}
