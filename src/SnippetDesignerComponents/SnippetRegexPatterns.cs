using System.Text.RegularExpressions;

namespace SnippetDesignerComponents
{
    public static class SnippetRegexPatterns
    {
        private const string replacmentPart = @"(("".*"")|(\w+))";
        private const string replacementStringFormat = @"((?<!{1}){1}{0}{1})|((?<={1}{0}{1}){1}{0}{1})";
        private const string potentialReplacementStringFormat = @"^{0}$";

        public static string BuildValidReplacementString(string delimiter)
        {
            var validReplacementString = string.Format(replacementStringFormat, replacmentPart, Regex.Escape(delimiter));
            return validReplacementString;
        }

        public static Regex BuildValidReplacementRegex(string delimiter)
        {
            var validReplacementString = BuildValidReplacementString(delimiter);
            return new Regex(validReplacementString, RegexOptions.Compiled);
        }

        public static Regex BuildValidPotentialReplacementRegex()
        {
            var validPotentialReplacement = string.Format(potentialReplacementStringFormat, replacmentPart);
            return new Regex(validPotentialReplacement, RegexOptions.Compiled);
        }
    }
}