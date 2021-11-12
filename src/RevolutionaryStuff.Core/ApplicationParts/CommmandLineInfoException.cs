namespace RevolutionaryStuff.Core.ApplicationParts;

public enum CommandLineInfoExceptionCodes
{
    MissingMandatoryArg,
    ExtraNonMappedArgsFound,
    DuplicateArgsFound,
    NoMandatesMet,
}

public class CommmandLineInfoException : CodedException<CommandLineInfoExceptionCodes>
{
    #region Constructors

    public CommmandLineInfoException(CommandLineInfoExceptionCodes code)
        : base(code)
    { }

    public CommmandLineInfoException(CommandLineInfoExceptionCodes code, Exception inner)
        : base(code, inner)
    { }

    public CommmandLineInfoException(CommandLineInfoExceptionCodes code, string message)
        : base(code, message)
    { }

    #endregion
}
