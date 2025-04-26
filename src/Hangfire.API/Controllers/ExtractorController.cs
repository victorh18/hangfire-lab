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
            _videoDownloader.EnqueueVideoDownload(request.Id, request.VideoUrl);
            return Ok("Hi mom!");
        }
    }
}
