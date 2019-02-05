using RevolutionaryStuff.Core;
using System;
using System.IO;

namespace RevolutionaryStuff.TheLoader
{
    public enum FileFormats
    {
        Auto,
        CSV,
        Pipe,
        CustomText,
        FixedWidthText,
        FoxPro,
        Excel,
        MySqlDump,
        Json,
        OData4,
        Html,
        ELF, //https://en.wikipedia.org/wiki/Extended_Log_Format
    }

    public static class FileFormatHelpers
    {
        public static FileFormats GetImpliedFormat(string filePath, Uri source)
        {
            var ext = Path.GetExtension(source?.AbsolutePath ?? filePath).ToLower();
            switch (ext)
            {
                case ".htm":
                case ".html":
                    return FileFormats.Html;
                case ".dbf":
                    return FileFormats.FoxPro;
                case ".csv":
                    return FileFormats.CSV;
                case ".pipe":
                    return FileFormats.Pipe;
                case ".log":
                    return FileFormats.ELF;
                case ".xls":
                case ".xlsx":
                    return FileFormats.Excel;
                case ".mdmp":
                    return FileFormats.MySqlDump;
                case ".json":
                    return FileFormats.Json;
                default:
                    throw new UnexpectedSwitchValueException(ext);
            }
        }
    }
}
