using System;

namespace Hangfire.API.Dtos.Extractor;

public class VideoExtractorRequest
{
    public string Id { get; set; }
    public string VideoUrl { get; set; }

}
