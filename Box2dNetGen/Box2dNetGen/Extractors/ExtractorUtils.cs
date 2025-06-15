using System.Text.RegularExpressions;

namespace Box2dNetGen.Extractors
{
    internal class ExtractorUtils
    {
        /// <summary>
        /// Gets the values of all regex matches, as a List.
        /// </summary>
        public static List<string> GetMultiCaptures(Group matchGroup)
        {
            return matchGroup.Captures.Select(c => c.Value.Trim()).ToList();
        }

        
    }
}
