using Hangfire.API.Dtos.Extractor;
using Hangfire.Application.VideoDownloader;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hangfire.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExtractorController : ControllerBase
    {
        private readonly IVideoDownloader _videoDownloader;

        public ExtractorController(IVideoDownloader videoDownloader)
        {
            _videoDownloader = videoDownloader;
        }
        [HttpPost]
        public async Task<IActionResult> QueueExtraction(VideoExtractorRequest request)
        {
            var id = DateTime.Now.ToString("yyyyMMdd_hhmmss");
            _videoDownloader.EnqueueVideoDownload(id, request.VideoUrl, request.TimeStamps.StartTime, request.TimeStamps.EndTime);
            return Ok($"{id}");
        }
    }
}
