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

namespace StudentManagementASP.Areas.Student.Controllers
{
    [Area("Student")]
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



    }
}
