using System.Text.RegularExpressions;

namespace SnippetDesignerComponents
{
    public static class SnippetRegexPatterns
    {
        public static string ValidPotentialReplacement;
        public static string ValidReplacementString;
        public static readonly Regex ValidPotentialReplacementRegex;
        public static readonly Regex ValidReplacementRegex;

        static SnippetRegexPatterns()
        {
            const string replacmentPart = @"(("".*"")|(\w+))";
            const string replacementStringFormat = @"((?<!\$)\${0}\$)|((?<=\${0}\$)\${0}\$)";
            const string potentialReplacementStringFormat = @"^{0}$";

            ValidReplacementString = string.Format(replacementStringFormat, replacmentPart);
            ValidReplacementRegex = new Regex(ValidReplacementString, RegexOptions.Compiled);

            ValidPotentialReplacement = string.Format(potentialReplacementStringFormat, replacmentPart);
            ValidPotentialReplacementRegex = new Regex(ValidPotentialReplacement, RegexOptions.Compiled);
        }
    }
}