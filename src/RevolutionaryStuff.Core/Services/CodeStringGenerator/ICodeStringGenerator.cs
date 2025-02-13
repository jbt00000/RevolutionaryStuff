namespace RevolutionaryStuff.Core.Services.CodeStringGenerator;
public interface ICodeStringGenerator
{
    string CreateCode(IList<char> characterSet, int numChars);

    static class CommonCharacterSets
    {
        public static class Roman
        {
            public static readonly IList<char> CaptchaCharset = new List<char>("ABCDEFGHJKMNPQRSTUVWXYZ23456789".ToCharArray()).AsReadOnly();
            public static readonly IList<char> DigitsCharset = new List<char>("0123456789".ToCharArray()).AsReadOnly();
            public static readonly IList<char> UpperCharset = new List<char>("ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray()).AsReadOnly();
            public static readonly IList<char> LowerCharset = new List<char>("abcdefghijklmnopqrstuvwxyz".ToCharArray()).AsReadOnly();
            public static readonly IList<char> LowerAndDigitsCharset = DigitsCharset.Union(LowerCharset).ToList().AsReadOnly();
            public static readonly IList<char> UpperAndDigitsCharset = DigitsCharset.Union(UpperCharset).ToList().AsReadOnly();
        }
    }

    string CreateCaptcharCharactersCode(int numChars)
        => CreateCode(CommonCharacterSets.Roman.CaptchaCharset, numChars);

    string CreateRomanLowerCharactersCode(int numChars)
        => CreateCode(CommonCharacterSets.Roman.LowerCharset, numChars);

    string CreateRomanNumericCode(int numChars)
        => CreateCode(CommonCharacterSets.Roman.DigitsCharset, numChars);
}
