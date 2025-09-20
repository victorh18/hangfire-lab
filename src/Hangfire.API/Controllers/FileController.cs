using Hangfire.Application.FileHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hangfire.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly IFileHandling _fileHandling;
        public FileController(IFileHandling fileHandling)
        {
            _fileHandling = fileHandling;
        }
        [Route("{id}")]
        public async Task<IActionResult> GetFile([FromRoute] string id)
        {
            try
            {
                var (fileBytes, mimeType, fileName) = await _fileHandling.GetFileFromId(id);

                // Return the file with appropriate content type
                return File(fileBytes, mimeType, fileName);
            }
            catch (FileNotFoundException)
            {
                return NotFound($"File with id '{id}' not found.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving file: {ex.Message}");
            }
        }
    }
}
