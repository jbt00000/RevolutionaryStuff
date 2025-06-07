namespace RevolutionaryStuff.Storage;

public interface IWebAccess
{
    Task<Uri> GenerateExternalUrlAsync(ExternalAccessSettings settings = null);
}
