using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using StudentManagementASP.Models;
using StudentManagementASP.Services;
using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;

namespace StudentManagementASP.Areas.Device.Controllers
{
    [Area("Device")]
    [Authorize(Roles = "attendance")]
    public class HomeController : Controller
    {
        private readonly StudentManagementContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly CheckFacePositionService _checkFaceService;
        public HomeController(StudentManagementContext context, IWebHostEnvironment environment, CheckFacePositionService checkFaceService)
        {
            _context = context;
            _environment = environment;
            _checkFaceService = checkFaceService;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CheckFacePosition([FromBody] JsonElement data)
        {
            // Kiểm tra xem phần tử JSON có chứa trường 'image' hay không
            if (data.TryGetProperty("image", out JsonElement imageElement))
            {
                string image = imageElement.GetString();
                var result = await _checkFaceService.CheckFacePosition(image);
                if(result.Trim() == "true")
                {
                    var base64Data = image.Replace("data:image/png;base64,", "");
                    var imageBytes = Convert.FromBase64String(base64Data);

                    // Tạo thư mục TempImages nếu chưa tồn tại
                    var tempFolder = Path.Combine("Scripts", "Data", "captured_faces", "attendance");
                    Directory.CreateDirectory(tempFolder); // Tạo thư mục nếu chưa tồn tại

                    // Tạo đường dẫn tạm thời cho ảnh
                    var tempImagePath = Path.Combine(tempFolder, "tempAttendance.png");
                    System.IO.File.WriteAllBytes(tempImagePath, imageBytes);
                    
                    // Call Python script to register user
                    var scriptPath = Path.Combine("Scripts", "Python", "mark_attendance.py");
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = "python",
                        Arguments = $"\"{scriptPath}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    };

                    using (var process = Process.Start(startInfo))
                    {
                        using (var reader = process.StandardOutput)
                        using (var errorReader = process.StandardError)
                        {
                            var output = await reader.ReadToEndAsync();
                            var errorOutput = await errorReader.ReadToEndAsync();

                            try
                            {
                                output = output.Trim();
                                if (output != "None")
                                {
                                    var student = _context.Students.AsNoTracking().FirstOrDefault(x => x.Id == output);
                                    if(student != null)
                                    {
                                        return Ok(new { success = true, id = student.Id, name = student.FullName });
                                    }
                                    else
                                    {
                                        return Ok(new { success = false });
                                    }
                                }

                                return BadRequest(new { message = "Python script returned an unsuccessful result.", details = errorOutput });
                            }
                            catch (Exception ex)
                            {
                                return BadRequest(new { message = "Error parsing the script output.", details = ex.Message });
                            }
                        }
                    }

                }
                return Json(new { error = true });
            }
            else
            {
                return Json(new { error = true });
            }
        }
    }
}
