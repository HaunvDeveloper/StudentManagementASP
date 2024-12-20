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

namespace StudentManagementASP.Controllers
{
    [Authorize(Roles = "student")]
    public class StudentController : Controller
    {
        private readonly StudentManagementContext _context;
        private readonly IWebHostEnvironment _environment;
        public StudentController(StudentManagementContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
            ExcelPackage.LicenseContext = LicenseContext.Commercial;
        }

        [HttpPost]
        public async Task<IActionResult> UploadExcel(int classid, int curriculumid, int majorid, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var students = new List<Student>();

            using (var package = new ExcelPackage(file.OpenReadStream()))
            {
                var worksheet = package.Workbook.Worksheets[0];
                var rowCount = worksheet.Dimension.Rows;

                Random r = new Random();
                for (int row = 7; row <= rowCount; row++)
                {
                    string MSSV = worksheet.Cells[row, 5].Text;
                    if (!string.IsNullOrEmpty(MSSV.Trim()))
                    {
                        MSSV = MSSV.Substring(0, 2) + "." + MSSV.Substring(2, 2) + "." + MSSV.Substring(4, 3) + "." + MSSV.Substring(7, 3);
                        Console.WriteLine(MSSV);
                        if (MSSV.Length != 13)
                            continue;
                        DateTime birthday = new DateTime(2004, r.Next(1, 12), r.Next(1, 28));
                        var student = new Student
                        {
                            FullName = worksheet.Cells[row, 2].Text,
                            DayOfBirth = birthday,
                            Email = worksheet.Cells[row, 6].Text,
                            Id = MSSV,
                            Status = "Còn học",
                            StudentClassId = classid,
                            CurriculumId = curriculumid,
                            MajorId = majorid,
                            DeptId = 1,
                            User = new User
                            {
                                Username = MSSV,
                                Password = MSSV,
                                FullName = worksheet.Cells[row, 2].Text,
                                DayOfBirth = birthday,
                                Email = worksheet.Cells[row, 6].Text,
                                IsBlock = false,
                                AuthId = 4
                            }
                        };

                        students.Add(student);
                    }
                }
            }

            // Loại bỏ các Student có ID trùng lặp
            var distinctStudents = students
                .GroupBy(s => s.Id)            // Nhóm các sinh viên theo Id
                .Select(g => g.First())         // Chỉ lấy sinh viên đầu tiên trong mỗi nhóm
                .ToList();

            // Lưu dữ liệu vào database
            _context.Students.AddRange(distinctStudents);
            await _context.SaveChangesAsync();

            return Ok("File uploaded and data saved successfully.");
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

        public IActionResult Dashboard()
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



    }
}
