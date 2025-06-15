using System.Text;
using System.Text.RegularExpressions;

namespace Box2dNetGen.Generators
{
    internal class CommentGenerator
    {
        private static Regex ParameterRegex = new("@param\\s+(?<identifier>\\S+)\\s+(?<description>.*)");

        public static void AppendComment(StringBuilder sb, List<string> comment, string? returnType, Dictionary<string, string>? extraParameterComments = null)
        {
            var originalParameterComments = new Dictionary<string, string>();
            if (comment.Count > 0)
            {
                sb.AppendLine("  /// <summary>");
                foreach (var s in comment)
                {
                    var parameterMatch = ParameterRegex.Match(s);
                    if (parameterMatch.Success)
                    {
                        originalParameterComments.Add(parameterMatch.Groups["identifier"].Value, parameterMatch.Groups["description"].Value);
                    }
                    else
                    {
                        sb.AppendLine("  /// " + s);
                    }
                }
                sb.AppendLine("  /// </summary>");
            }

            if (!string.IsNullOrWhiteSpace(returnType))
                sb.AppendLine($"  /// <returns>Original C type: {returnType}</returns>");

            AppendParameterComments(sb, originalParameterComments, extraParameterComments); // if any...
        }

        private static void AppendParameterComments(StringBuilder sb, Dictionary<string, string> originalParameterComments,
            Dictionary<string, string>? extraParameterComments)
        {
            var parameterIdentifiers = originalParameterComments.Keys.ToList();
            if (extraParameterComments != null) parameterIdentifiers.AddRange(extraParameterComments.Keys);
            foreach (var parameter in parameterIdentifiers)
            {
                var parameterCommentLines = new List<string>();
                if (originalParameterComments.TryGetValue(parameter, out var originalComment))
                    parameterCommentLines.Add(originalComment);
                if (extraParameterComments != null && extraParameterComments.TryGetValue(parameter, out var extraComment))
                    parameterCommentLines.Add(extraComment);
                sb.AppendLine($"  /// <param name=\"{parameter}\">{string.Join("\r\n  /// ", parameterCommentLines)}</param>");
            }
        }
    }
}
