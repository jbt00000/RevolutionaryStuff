namespace RevolutionaryStuff.Core.ApplicationParts
{
    public interface IDeleteOnSave
    {
        void Delete();
        bool IsMarkedForDeletion { get; }
    }
}
