using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OfficeOpenXml;
using StudentManagementASP.Models;
using StudentManagementASP.Services;
using StudentManagementASP.ViewModels;
using System.Security.Claims;
using System.IO;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using System.Diagnostics;

namespace StudentManagementASP.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "student")]
    public class StudentController : Controller
    {
        private readonly StudentManagementContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly CheckFacePositionService _checkFaceService;
        public StudentController(StudentManagementContext context, IWebHostEnvironment environment, CheckFacePositionService checkFaceService)
        {
            _context = context;
            _environment = environment;
            ExcelPackage.LicenseContext = LicenseContext.Commercial;
            _checkFaceService = checkFaceService;
        }




        public IActionResult Info()
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var student = _context.Students.AsNoTracking().FirstOrDefault(x => x.UserId == userId);
            if (student == null)
            {
                return NotFound();
            }
            return View(student);
        }

        public IActionResult CreateFaceIdentify()
        {
            return View();
        }


        public IActionResult EditInfo()
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var student = _context.Students.AsNoTracking()
                .FirstOrDefault(x => x.UserId == userId);
            if (student == null)
            {
                return NotFound();
            }
            string filePath = Path.Combine(_environment.WebRootPath, "Data", "nation.json");
            var jsonData = System.IO.File.ReadAllText(filePath);
            var ethnicList = JsonConvert.DeserializeObject<List<NationViewModel>>(jsonData);
            ViewBag.NationNames = new SelectList(ethnicList, "EthnicName", "EthnicName", student.Nation);

            filePath = Path.Combine(_environment.WebRootPath, "Data", "religion.json");
            jsonData = System.IO.File.ReadAllText(filePath);
            var religionList = JsonConvert.DeserializeObject<List<ReligionViewModel>>(jsonData);
            ViewBag.ReligionNames = new SelectList(religionList, "ReligionName", "ReligionName", student.Religion);
            return View(student);
        }

        [HttpPost]
        public async Task<IActionResult> EditInfo(DateTime DayOfBirth, string Nation, int Province, int District, int Ward, string StreetAddress, string PhoneNo, string Email, string Religion, string BirthPlace)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var student = _context.Students.FirstOrDefault(x => x.UserId == userId);
            if (student == null)
            {
                return NotFound();
            }
            student.DayOfBirth = DayOfBirth;
            student.Email = Email;
            student.BirthPlace = BirthPlace;
            student.Nation = Nation;
            student.PhoneNo = PhoneNo;
            student.DistrictCode = District;
            student.WardCode = Ward;
            student.StreetAddress = StreetAddress;
            student.ProvinceCode = Province;
            student.Religion = Religion;
            await _context.SaveChangesAsync();
            return RedirectToAction("Info", "Student");
        }

        [HttpPost]
        public async Task<IActionResult> CheckFacePosition([FromBody] JsonElement data)
        {
            // Kiểm tra xem phần tử JSON có chứa trường 'image' hay không
            if (data.TryGetProperty("image", out JsonElement imageElement))
            {
                string image = imageElement.GetString();
                var result = await _checkFaceService.CheckFacePosition(image);
                return Json(new { isCentered = result.Trim() == "true" });
            }
            else
            {
                return Json(new { error = true });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterUserModel model)
        {
            if (model.Images == null || model.Images.Count < 5)
                return BadRequest(new { message = "Insufficient images for registration." });
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var student = _context.Students.FirstOrDefault(x => x.UserId == userId);
            if (student == null)
            {
                return NotFound();
            }
            model.UserId = student.Id;

            var tempFolder = Path.Combine(_environment.WebRootPath, "TempImages", student.Id);
            Directory.CreateDirectory(tempFolder);

            for (int i = 0; i < model.Images.Count; i++)
            {
                var base64Data = model.Images[i].Replace("data:image/png;base64,", "");
                var imageBytes = Convert.FromBase64String(base64Data);
                var imagePath = Path.Combine(tempFolder, $"image_{i + 1}.png");
                await System.IO.File.WriteAllBytesAsync(imagePath, imageBytes);
            }

            // Call Python script to register user
            var scriptPath = Path.Combine("Scripts", "Python", "register.py");
            var startInfo = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = $"\"{scriptPath}\" \"{model.UserId}\"",
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
                        
                        if (output.Contains("successCode:ABCDE"))
                        {
                            student.FaceData = "Success";
                            _context.SaveChanges();
                            return Ok(new {success = true, redirect= Url.Action("Info") }); 
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

    }
}
