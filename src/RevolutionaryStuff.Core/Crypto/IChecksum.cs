namespace RevolutionaryStuff.Core.Crypto
{
    public interface IChecksum
    {
        long Checksum { get; }
        void Update(byte[] buf, int offset, int length);
    }
}
