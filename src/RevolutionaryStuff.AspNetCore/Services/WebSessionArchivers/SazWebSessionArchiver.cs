using System.IO;
using System.IO.Compression;
using Microsoft.AspNetCore.Http;

namespace RevolutionaryStuff.AspNetCore.Services.SazGenerators;

internal class SazWebSessionArchiver : ISazWebSessionArchiver
{
    string IWebSessionArchiver.FileExtension => ".saz";

    async Task IWebSessionArchiver.GenerateAsync(IList<WebSession> sessions, Stream outputStream)
    {
        Requires.WriteableStreamArg(outputStream);

        using var zipArchive = new ZipArchive(outputStream, ZipArchiveMode.Create, true);
        if (sessions.HasData())
        {
            var places = sessions.Count.ToString().Length;
            var idFormat = $"0{{0:D{places}}}";
            var sessionNum = 0;
            foreach (var session in sessions.NullSafeEnumerable())
            {
                var id = string.Format(idFormat, ++sessionNum);
                if (session.Request != null)
                {
                    var zipEntry = zipArchive.CreateEntry($"raw/{id}_c.txt");
                    await WriteClientRequestAsync(session.Request, session.RequestBody ?? session.Request.Body, zipEntry.Open());
                }
            }
        }
    }

    private async Task WriteClientRequestAsync(HttpRequest request, Stream requestBody, Stream outputStream)
    {
        var textWriter = (TextWriter)new StreamWriter(outputStream);
        await textWriter.WriteAsync($"{request.Method} {request.Scheme}://{request.Host}{request.Path}{request.QueryString} {request.Protocol}\r\n");
        foreach (var kvp in request.Headers)
        {
            await textWriter.WriteAsync($"{kvp.Key}: {kvp.Value}\r\n");
        }
        await textWriter.WriteAsync("\r\n");
        await textWriter.FlushAsync();

        if (request.Method != WebHelpers.Methods.Get && request.Method != WebHelpers.Methods.Head && request.Method != WebHelpers.Methods.Options && requestBody != null)
        {
            await requestBody.CopyToAsync(outputStream);
            if (requestBody.CanSeek)
            {
                requestBody.Position = 0;
            }
        }
    }
}
