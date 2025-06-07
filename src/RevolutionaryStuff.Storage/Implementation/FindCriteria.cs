namespace RevolutionaryStuff.Storage.Implementation;

/// <summary>
/// FindCriteria is an implementation of <see cref="IFindCriteria" />
/// that enables a consumer to specify the criteria they're using
/// for a search.
/// </summary>
public class FindCriteria : IFindCriteria
{
    /// <summary>
    /// The default number of items to return in a page of results.
    /// </summary>
    public const int DefaultPageSize = 100;

    /// <summary>
    /// The default pattern for matching files, which will match
    /// anything.
    /// </summary>
    public const string DefaultMatchPattern = ".+";

    /// <summary>
    /// This criteria is used for matching everything in the current folder.
    /// It uses defaults for the <see cref="FindCriteria" /> instance's
    /// properties.
    /// </summary>
    public static readonly IFindCriteria AllItemsCurrentFolderFindCriteria =
        new FindCriteria
        {
            MatchCasing = MatchCasingEnum.CaseInsensitive,
            MatchPattern = DefaultMatchPattern,
            PatternFormat = MatchPatternFormatEnum.RegularExpressionPattern,
            PageSize = DefaultPageSize,
            NestingOption = MatchNestingOptionEnum.CurrentFolder
        };

    /// <summary>
    /// This criteria is used for matching everything in all folders.
    /// It uses defaults for the <see cref="FindCriteria" /> instance's
    /// properties.
    /// </summary>
    public static readonly IFindCriteria AllItemsAllFolderFindCriteria =
        new FindCriteria
        {
            MatchCasing = MatchCasingEnum.CaseInsensitive,
            MatchPattern = DefaultMatchPattern,
            PatternFormat = MatchPatternFormatEnum.RegularExpressionPattern,
            PageSize = DefaultPageSize,
            NestingOption = MatchNestingOptionEnum.CurrentFolderAndChildFolders
        };

    /// <summary>
    /// This criteria finds everything in the current folder, with
    /// default values for the <see cref="FindCriteria" /> instance's
    /// properties.
    /// </summary>
    public static readonly IFindCriteria DefaultFindCriteria =
        AllItemsCurrentFolderFindCriteria;

    /// <summary>
    /// The <see cref="MatchNestingOptionEnum" /> option, which determines
    /// whether to search the current folder or the current folder and
    /// all folders.
    /// </summary>
    public MatchNestingOptionEnum NestingOption { get; set; } =
        MatchNestingOptionEnum.CurrentFolder;

    /// <summary>
    /// The <see cref="MatchPatternFormatEnum" /> pattern choice, which
    /// dictates whether the file pattern that you're searching for
    /// is a glob, a regex, or an exact match.
    /// </summary>
    public MatchPatternFormatEnum PatternFormat { get; set; } =
        MatchPatternFormatEnum.RegularExpressionPattern;

    /// <summary>
    /// Whether the name casing is case-sensitive or -insensitive.
    /// </summary>
    public MatchCasingEnum MatchCasing { get; set; } =
        MatchCasingEnum.CaseInsensitive;

    /// <summary>
    /// What part needs to be tested
    /// </summary>
    public MatchTargetEnum MatchTarget { get; set; } =
        MatchTargetEnum.Name;

    /// <summary>
    /// The pattern to search for, which defaults to the
    /// <see cref="DefaultMatchPattern" />.
    /// </summary>
    public string MatchPattern { get; set; } = DefaultMatchPattern;

    /// <summary>
    /// The number of items to return in a page, which defaults to
    /// <see cref="DefaultPageSize" />.
    /// </summary>
    public int PageSize { get; set; } = DefaultPageSize;
}
