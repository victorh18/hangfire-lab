using System;

namespace Hangfire.Application.FileHandling;

public interface IFileHandling
{
    Task<byte[]> GetFileFromId(string id);
}

public class FileHandling : IFileHandling
{
    public async Task<byte[]> GetFileFromId(string id)
    {
        const string BASE_PATH = "/Users/Videlarosa/Projects/personal/hangfire-lab/src/Hangfire.Worker/";
        var filePath = Path.Combine(BASE_PATH, $"{id}.gif");

        return await File.ReadAllBytesAsync(filePath);
    }
}
