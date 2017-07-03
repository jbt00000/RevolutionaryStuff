
namespace RevolutionaryStuff.Core.ApplicationParts
{
    /// <summary>
    /// A "PreSave" should be called before an external persistence occurs
    /// </summary>
    public interface IPreSave
    {
        /// <summary>
        /// Should be called before the entity is saved
        /// </summary>
        void PreSave();
    }
}
