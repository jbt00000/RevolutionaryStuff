using System.IO;
using Microsoft.AspNetCore.Http;

namespace RevolutionaryStuff.AspNetCore.Services.SazGenerators;

public interface IWebSessionArchiver
{
    /// <summary>
    /// Generate a .saz file to be read into fiddler.
    /// </summary>
    /// <param name="sessions">The sessions to be saved.</param>
    /// <param name="outputStream">The output stream that holds the generated .saz file.</param>
    /// <returns>Waitable task</returns>
    Task GenerateAsync(IList<WebSession> sessions, Stream outputStream);

    /// <summary>
    /// The standard file extension to use with this archive
    /// </summary>
    string FileExtension { get; }

    #region Default Implementations

    Task GenerateAsync(HttpRequest request, Stream outputStream)
        => GenerateAsync(new WebSession(request), outputStream);

    Task GenerateAsync(WebSession session, Stream outputStream)
        => GenerateAsync(new[] { session }, outputStream);

    #endregion
}
