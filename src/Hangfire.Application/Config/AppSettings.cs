using System;
using System.ComponentModel.DataAnnotations;

namespace Hangfire.Application.Config;

public class AppSettings
{
    public required ConnectionStrings ConnectionStrings { get; set; }
    public required HostUrls HostUrls { get; set; }
    public required FilePaths FilePaths { get; set; }
}

public class ConnectionStrings
{
    public required string Hangfire { get; set; }
}

public class HostUrls
{
    public required string InternalWebSocket { get; set; }
}

public class FilePaths
{
    public required string DownloadPath { get; set; }
    public required string ResultsPath { get; set; }
}
