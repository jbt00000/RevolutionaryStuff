namespace RevolutionaryStuff.Core.ApplicationParts;

public enum CommandLineSwitchAttributeTranslators
{
    None = 0,
    Csv = 0b1,
    FilePath = 0b10,
    NameValuePairs = 0b100,
    Csints = 0b1000,
    Url = 0b10000,
    FilePathOrUrl = 0b100000,
}
