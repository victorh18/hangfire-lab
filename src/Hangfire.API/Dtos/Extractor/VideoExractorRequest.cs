using System;
using System.ComponentModel.DataAnnotations;
using Hangfire.Application.Common;

namespace Hangfire.API.Dtos.Extractor;

public class VideoExtractorRequest
{
    public string Id { get; set; }
    public string VideoUrl { get; set; }

    [Required]
    public VideoTimeStamp TimeStamps { get; set; }
    public ExtractionType ExtractionType { get; set; }

}

public record VideoTimeStamp(string StartTime, string EndTime);