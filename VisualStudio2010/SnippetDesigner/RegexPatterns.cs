using System.Text.RegularExpressions;

namespace Microsoft.SnippetDesigner
{
    public static class RegexPatterns
    {
        public const string ValidPotentialReplacementString = @"^(("".*"")|(\w+))$";
                                                        //@"(?<!\$)\$(("".*"")|(\w+))\$";
        public const string ValidIndividualReplacementString = @"^\$(("".*"")|(\w+))\$$";
        public const string ValidReplacementString = @"\$(("".*"")|(\w+))\$";
        public static readonly Regex ValidIndividualReplacementRegex = new Regex(ValidIndividualReplacementString, RegexOptions.Compiled);
        public static readonly Regex ValidPotentialReplacementRegex = new Regex(ValidPotentialReplacementString, RegexOptions.Compiled);
        public static readonly Regex ValidReplacementRegex = new Regex(ValidIndividualReplacementString, RegexOptions.Compiled);
    }
}