using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using StudentManagementASP.Models;
using StudentManagementASP.Services;
using System.Drawing;
using System.Security.Claims;

namespace StudentManagementASP.Controllers
{
    [Authorize(Roles = "student")]
    public class StudentController : Controller
    {
        private readonly StudentManagementContext _context;
        public StudentController(StudentManagementContext context)
        {
            _context = context;
            ExcelPackage.LicenseContext = LicenseContext.Commercial;
        }

        public IActionResult Index()
        {
            ViewBag.ClassStudent = _context.StudentClasses
                .AsNoTracking()
                .Select(x => new SelectListItem()
                {
                    Value = x.Id.ToString(),
                    Text = x.Code + " - " + x.Name,
                })                    
                .ToList();
            ViewBag.Curriculum = _context.Curricula
                .AsNoTracking()
                .Select(x => new SelectListItem()
                {
                    Value = x.Id.ToString(),
                    Text = x.Code + " - " + x.Name,
                })
                .ToList();

            ViewBag.Major = new SelectList(_context.Majors.AsNoTracking().ToList(), "Id", "Name");
            return View();
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

        public IActionResult EditInfo()
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var student = _context.Students.AsNoTracking().FirstOrDefault(x => x.UserId == userId);
            if (student == null)
            {
                return NotFound();
            }
            return View(student);
        }

        [HttpPost]
        public IActionResult EditInfo(DateTime DayOfBirth, string Nation, int Province, int District, int Ward, string StreetAddress, string PhoneNo, string Email)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var student = _context.Students.FirstOrDefault(x => x.UserId == userId);
            if (student == null)
            {
                return NotFound();
            }
            student.DayOfBirth = DayOfBirth;
            student.Email = Email;
            var info = student.StudentInfos.FirstOrDefault();
            if (info != null)
            {
                
            }
            return View(student);
        }

    }
}
