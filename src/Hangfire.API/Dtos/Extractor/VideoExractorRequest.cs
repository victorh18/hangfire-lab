using System;
using System.ComponentModel.DataAnnotations;

namespace Hangfire.API.Dtos.Extractor;

public class VideoExtractorRequest
{
    public string Id { get; set; }
    public string VideoUrl { get; set; }

    [Required]
    public VideoTimeStamp TimeStamps { get; set; }

}

public record VideoTimeStamp(string StartTime, string EndTime);