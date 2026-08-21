using BlobStorageApplication.Model;
using BlobStorageApplication.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BlobStorageApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly IFileService _fileService;

        public FileController(IFileService fileService)
        {
            this._fileService = fileService;
        }

        [HttpPost]
        [Route("upload")]
        public async Task<IActionResult> Upload([FromForm] FileModel fileModel)
        {
            await this._fileService.Upload(fileModel);
            return Ok(fileModel);
        }

        [HttpPost]
        [Route("download")]
        public async Task<IActionResult> Download([FromQuery] string name)
        {
            var fileStream = await this._fileService.Download(name);
            string fileType = "png";
            if (name.Contains("jpeg"))
            {
                fileType = "jpeg";
            }
            return File(fileStream, $"image/{fileType}", $"image/{fileType}");
        }
    }
}
