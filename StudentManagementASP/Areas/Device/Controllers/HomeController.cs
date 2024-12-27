using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using StudentManagementASP.Hubs;
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
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var device = _context.Devices.AsNoTracking().FirstOrDefault(x => x.UserId == userId);
            return View(device);
        }

        public IActionResult CheckActivate()
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var device = _context.Devices.AsNoTracking().FirstOrDefault(x => x.UserId == userId);
            return Json(new { success = device?.IsActive });
        }

        [HttpPost]
        public async Task<IActionResult> CheckFacePosition([FromBody] JsonElement data)
        {
            // Kiểm tra xem phần tử JSON có chứa trường 'image' hay không
            if (data.TryGetProperty("image", out JsonElement imageElement))
            {
                string image = imageElement.GetString();
                var result = await _checkFaceService.CheckFacePosition(image);
                
                return Json(new { success = result.Trim() == "true" });
            }
            else
            {
                return Json(new { error = true });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Verify([FromBody] JsonElement data)
        {
            if (data.TryGetProperty("image", out JsonElement imageElement))
            {
                string image = imageElement.GetString();
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
                                if (student != null)
                                {
                                    

                                    var now = DateTime.Now;
                                    int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                                    var device = _context.Devices.AsNoTracking().FirstOrDefault(x => x.UserId == userId);
                                    if (device != null)
                                    {
                                        var lesson = device.Lesson;
                                        
                                        if (lesson == null) { return Ok(new { success = false }); }

                                        var exist = _context.StudentJoinLessons.AsNoTracking().FirstOrDefault(x => x.StudentId == student.Id && x.LessonId == lesson.Id);
                                        if (exist != null) { return Ok(new { success = false, error = "Đã điểm danh" }); }

                                        var attendTime = lesson.StartLessonNavigation.StartTime;
                                        TimeOnly timeOnly = TimeOnly.FromDateTime(now);
                                        string status = (timeOnly > attendTime ? "Đi trễ" : "Có mặt");
                                        _context.StudentJoinLessons.Add(new StudentJoinLesson
                                        {
                                            StudentId = student.Id,
                                            LessonId = lesson.Id,
                                            JoinTime = now,
                                            Status = status,

                                        });
                                        await _context.SaveChangesAsync();
                                        var hubContext = HttpContext.RequestServices.GetRequiredService<IHubContext<AttendanceHub>>();
                                        await hubContext.Clients.All.SendAsync("ReceiveAttendance", student.Id, status);
                                    }

                                    return Ok(new { success = true, id = student.Id, name = student.FullName });
                                }
                                else
                                {
                                    return Ok(new { success = false });
                                }
                            }

                            return Ok(new { success = false }); ;
                        }
                        catch (Exception ex)
                        {
                            return Ok(new { success = false });
                        }
                    }
                }
            }
            return Ok(new { success = false });

        }


    }
}
