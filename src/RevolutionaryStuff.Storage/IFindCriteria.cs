namespace RevolutionaryStuff.Storage;

/// <summary>
/// The IFindCriteria interface is used for defining criteria for
/// searching for folders and files.
/// </summary>
public interface IFindCriteria
{
    /// <summary>
    /// The type of search to conduct.
    /// </summary>
    MatchNestingOptionEnum NestingOption { get; }

    MatchPatternFormatEnum PatternFormat { get; set; }

    MatchCasingEnum MatchCasing { get; set; }

    MatchTargetEnum MatchTarget { get; set; }

    /// RYANSfile
    /// ryansFile
    /// Flags: MsDos | CaseInsensitive
    /// <summary>
    /// The MatchPattern is the pattern you're searching for.
    /// someFileName
    /// Valid examples:
    /// - /someFileName will match any file with the pattern someFileName in it.
    /// - /someFileName
    /// NOTE: this is case insensitive due to the i flag at the end of the pattern.
    /// - /.*/ will match any file name.
    /// Invalid examples:
    /// - *.* won't work. This is a ms-dos filesystem search.
    /// NOTE: the only regular expression option that is currently supported is i,
    /// which enables case insensitive searches.
    /// </summary>
    string MatchPattern { get; }
}
