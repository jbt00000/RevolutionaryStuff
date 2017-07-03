namespace RevolutionaryStuff.Core
{
    public sealed class DoNotCare
    {
        public override int GetHashCode() => 0;

        public override bool Equals(object obj) => obj is DoNotCare;
    }
}
