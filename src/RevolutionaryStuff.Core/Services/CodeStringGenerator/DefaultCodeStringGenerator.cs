using System.Text;

namespace RevolutionaryStuff.Core.Services.CodeStringGenerator;

public class DefaultCodeStringGenerator : ICodeStringGenerator
{
    public static readonly ICodeStringGenerator Instance = new DefaultCodeStringGenerator();

    private readonly Random R;

    public DefaultCodeStringGenerator()
        : this(Stuff.RandomWithRandomSeed)
    {
        R = Stuff.RandomWithRandomSeed;
    }

    public DefaultCodeStringGenerator(Random r)
    {
        ArgumentNullException.ThrowIfNull(r);

        R = r;
    }

    string ICodeStringGenerator.CreateCode(IList<char> characterSet, int numChars)
    {
        Requires.Positive(numChars);
        StringBuilder chars = new(numChars);
        for (var z = 0; z < numChars; ++z)
        {
            chars.Append(characterSet[R.Next(characterSet.Count)]);
        }
        return chars.ToString();
    }
}
