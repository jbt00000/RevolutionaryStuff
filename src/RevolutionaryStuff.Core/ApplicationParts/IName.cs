namespace RevolutionaryStuff.Core.ApplicationParts
{
    public interface IName
    {
        string Name { get; }
    }

    public static class NameHelpers
    {
        public static string GetName(object o, string fallback = null)
        {
            return (o as IName)?.Name ?? o?.GetType().Name ?? fallback;
        }
    }
}
