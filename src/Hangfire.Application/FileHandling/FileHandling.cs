using System;
using Hangfire.Application.Config;
using Microsoft.Extensions.Options;

namespace Hangfire.Application.FileHandling;

public interface IFileHandling
{
    Task<(byte[] fileBytes, string mimeType, string fileName)> GetFileFromId(string id);
}

public class FileHandling : IFileHandling
{
    private readonly IOptions<AppSettings> _options;
    const string BASE_PATH = "/Users/Videlarosa/Projects/personal/hangfire-lab/src/Hangfire.Worker/";
    private readonly Dictionary<string, string> mimeTypes = new()
    {
        {".mp3", "audio/mp3"},
        {".gif", "image/gif"}
    };

    public FileHandling(IOptions<AppSettings> options)
    {
        _options = options;
    }

    public async Task<(byte[] fileBytes, string mimeType, string fileName)> GetFileFromId(string id)
    {
        var filePath = FindFile(id);
        var extension = Path.GetExtension(filePath);
        var mimeType = mimeTypes.GetValueOrDefault(extension);
        var fileBytes = await File.ReadAllBytesAsync(filePath);
        var fileName = Path.GetFileName(filePath);

        return (fileBytes, mimeType, fileName);
    }

    private string FindFile(string id)
    {
        var file = Directory.GetFiles(_options.Value.FilePaths.ResultsPath)
            .Where(file => mimeTypes.Keys.Contains(Path.GetExtension(file)))
            .FirstOrDefault(file => Path.GetFileNameWithoutExtension(file) == id);
        if (file != null)
            return file;

        throw new Exception("File not found!");
    }
}
