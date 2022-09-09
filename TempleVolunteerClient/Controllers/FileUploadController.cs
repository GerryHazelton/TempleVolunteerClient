using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TempleVolunteerClient.Common;

namespace TempleVolunteerClient.Controllers
{
    public class FileUploadController : CustomController
    {
        public FileUploadController(IHttpContextAccessor httpContextAccessor, IOptions<AppSettings> AppSettings)
            : base(httpContextAccessor, AppSettings)
        {

        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("UploadFile")]
        public async Task<IActionResult> UploadFile (IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return Content("No file selected for upload.");
            }

            var filePath = Path.GetTempFileName();
            
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            
            return RedirectToAction("Home");
        }

        public async Task<IActionResult> DownloadFile(string path, string filename)
        {
            if (filename == null || filename.Length == 0)
            {
                return Content("No file selected for download.");
            }

            var filePath = Path.Combine(path, filename);
            var memoryStream = new MemoryStream();

            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                await stream.CopyToAsync(memoryStream);
            }
            
            memoryStream.Position = 0;
            return File(memoryStream, "text/plain",
            Path.GetFileName(path));
        }
    }
}
