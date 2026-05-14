using System.Text;
using System.Text.RegularExpressions;

namespace Box2dNetGen.Generators
{
    internal class CommentGenerator
    {
        private static Regex ParameterRegex = new("@param\\s+(?<identifier>\\S+)\\s+(?<description>.*)");

        public static void AppendComment(StringBuilder sb, List<string> comment, string? returnType,
            Dictionary<string, string>? extraParameterComments = null)
        {
            Dictionary<string, string> originalParameterComments = OriginalParameterComments(sb, comment, returnType);

            AppendParameterComments(sb, originalParameterComments, extraParameterComments); // if any...
        }

        public static void AppendOverloadComment(StringBuilder sb, List<string> comments, string? returnType,
            Dictionary<string, string> identifierToOriginalIdentifiers)
        {
            Dictionary<string, string> originalParameterComments = OriginalParameterComments(sb, comments, returnType);

            var parameterIdentifiers = originalParameterComments.Keys.ToList();
            parameterIdentifiers.AddRange(identifierToOriginalIdentifiers.Keys);

            foreach (var parameter in parameterIdentifiers)
            {
                var parameterCommentLines = new List<string>();
                if (originalParameterComments.TryGetValue(parameter, out var originalComment))
                    parameterCommentLines.Add(originalComment);
                if (identifierToOriginalIdentifiers.TryGetValue(parameter, out var extraComments))
                {
                    parameterCommentLines.Add(extraComments);
                }

                sb.AppendLine(
                    $"  /// <param name=\"{parameter}\">{string.Join("\r\n  /// ", parameterCommentLines)}</param>");
            }
        }

        static Dictionary<string, string> OriginalParameterComments(StringBuilder sb, List<string> comment,
            string? returnType)
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
                        originalParameterComments.Add(parameterMatch.Groups["identifier"].Value,
                            parameterMatch.Groups["description"].Value);
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
            return originalParameterComments;
        }

        private static void AppendParameterComments(StringBuilder sb,
            Dictionary<string, string> originalParameterComments,
            Dictionary<string, string>? extraParameterComments)
        {
            var parameterIdentifiers = originalParameterComments.Keys.ToList();
            if (extraParameterComments != null) parameterIdentifiers.AddRange(extraParameterComments.Keys);
            foreach (var parameter in parameterIdentifiers)
            {
                var parameterCommentLines = new List<string>();
                if (originalParameterComments.TryGetValue(parameter, out var originalComment))
                    parameterCommentLines.Add(originalComment);
                if (extraParameterComments != null
                    && extraParameterComments.TryGetValue(parameter, out var extraComment))
                    parameterCommentLines.Add(extraComment);
                sb.AppendLine(
                    $"  /// <param name=\"{parameter}\">{string.Join("\r\n  /// ", parameterCommentLines)}</param>");
            }
        }
    }
}