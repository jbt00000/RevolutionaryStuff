using System.Diagnostics;
using System.Text.RegularExpressions;
using DotNet.Globbing;

namespace RevolutionaryStuff.Storage.Implementation;

/// <summary>
/// This class provides static extension methods that extend the
/// functionality of <see cref="IFindCriteria" /> implementations.
/// </summary>
public static class FindCriteriaHelpers
{
    /// <summary>
    /// Determines whether or not a test string matches the find criteria.
    /// </summary>
    /// <param name="findCriteria">
    /// An <see cref="IFindCriteria" /> implementation that will be used
    /// for matching a string.
    /// </param>
    /// <param name="test">The string to test a match against.</param>
    /// <returns>true if the test string matches the criteria.</returns>
    public static bool IsMatch(this IFindCriteria findCriteria, string test)
        => IsMatch(findCriteria.MatchPattern, findCriteria.PatternFormat, findCriteria.MatchCasing, test);

    public static bool IsMatch(this IFindCriteria findCriteria, IEntry test)
    {
        var s = findCriteria.MatchTarget switch
        {
            MatchTargetEnum.Name => test.Name,
            MatchTargetEnum.RootRelativePath => test.Path,
            _ => throw new UnexpectedSwitchValueException(findCriteria
                .MatchTarget)
        };
        return IsMatch(findCriteria.MatchPattern, findCriteria.PatternFormat, findCriteria.MatchCasing, s);
    }


    /// <summary>
    /// Determine whether or not a string matches a pattern, given the
    /// pattern matching arguments.
    /// </summary>
    /// <param name="matchPattern">The pattern to use for matching.</param>
    /// <param name="patternFormat">
    /// The <see cref="MatchPatternFormatEnum" /> format to use for matching.
    /// </param>
    /// <param name="casing">Whether or not matching is case-sensitive.</param>
    /// <param name="test">The string to test against the match criteria.</param>
    /// <returns>true if the test string matches.</returns>
    /// <exception cref="NotSupportedException">
    /// This is thrown if a <see cref="MatchPatternFormatEnum" /> is used that
    /// isn't understood by this extension method.
    /// </exception>
    public static bool IsMatch(
        string matchPattern,
        MatchPatternFormatEnum patternFormat,
        MatchCasingEnum casing,
        string test
    )
    {
        Requires.Text(matchPattern);

        if (string.IsNullOrEmpty(test)) return false;

        bool ret;
        switch (patternFormat)
        {
            case MatchPatternFormatEnum.RegularExpressionPattern:
                var options = casing == MatchCasingEnum.CaseInsensitive
                    ? RegexOptions.IgnoreCase
                    : RegexOptions.None;
                ret = new Regex(matchPattern, options).IsMatch(test);
                break;
            case MatchPatternFormatEnum.ExactString:
                ret = 0 == string.Compare(matchPattern, test, casing == MatchCasingEnum.CaseInsensitive);
                break;
            case MatchPatternFormatEnum.Glob:
                ret = Glob.Parse(matchPattern,
                    new GlobOptions
                    {
                        Evaluation = new EvaluationOptions
                        {
                            CaseInsensitive =
                                casing == MatchCasingEnum.CaseInsensitive
                        }
                    }).IsMatch(test);
                break;
            default:
                throw new UnexpectedSwitchValueException(patternFormat);
        }

        Debug.WriteLine($"IsMatch(\"{matchPattern}\", \"{patternFormat}\", \"{test}\")=>{ret}");

        return ret;
    }
}
