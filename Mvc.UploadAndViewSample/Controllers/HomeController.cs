using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Mvc.UploadAndViewSample.Models;

namespace Mvc.UploadAndViewSample.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;

        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new ImageViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Index(IFormFile imageFile)
        {
            var viewModel = new ImageViewModel();

            if (imageFile != null && imageFile.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("imageFile", "Please select a valid image file (JPG, JPEG, PNG, GIF).");
                    return View(viewModel);
                }

                try
                {
                    string? saveFolder = _configuration.GetSection("Config:SaveFolder").Value;
                    string? uploadFileFolder = _configuration.GetSection("Config:UploadFileFolder").Value;

                    if (string.IsNullOrEmpty(saveFolder) || string.IsNullOrEmpty(uploadFileFolder))
                    {
                        ModelState.AddModelError(string.Empty, "Configuration for file storage is missing.");
                        return View(viewModel);
                    }

                    string physicalPath = Path.Combine(Directory.GetCurrentDirectory(), saveFolder, uploadFileFolder);

                    if (!Directory.Exists(physicalPath))
                    {
                        Directory.CreateDirectory(physicalPath);
                    }

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                    string fullPhysicalPath = Path.Combine(physicalPath, uniqueFileName);

                    using (var fileStream = new FileStream(fullPhysicalPath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    viewModel.ImageUrl = "/" + uploadFileFolder + "/" + uniqueFileName;
                    ViewBag.SuccessMessage = "Image uploaded successfully!";
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error uploading file: {ex.Message}");
                    ModelState.AddModelError(string.Empty, "An error occurred while uploading the file. Please try again.");
                }
            }
            else
            {
                ModelState.AddModelError("imageFile", "Please select a file to upload.");
            }

            return View(viewModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
