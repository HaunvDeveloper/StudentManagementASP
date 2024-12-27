using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using StudentManagementASP.Models;
using StudentManagementASP.Services;
using StudentManagementASP.ViewModels;
using System.Security.Claims;

namespace StudentManagementASP.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles ="student")]
    public class HomeController : Controller
    {
        private readonly StudentManagementContext _context;
        public HomeController(StudentManagementContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult GetChartData(int semesterId)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var student = _context.Students.AsNoTracking().FirstOrDefault(x => x.UserId == userId);
            if (student == null) { return NotFound(); }
            var chart = new ChartViewModel();
            var studentJoinLesson = _context.StudentJoinLessons.AsNoTracking()
                .Include(x => x.Lesson)
                    .ThenInclude(x => x.CourseClass)
                .Where(x => x.StudentId == student.Id && x.Lesson.CourseClass.SemesterId == semesterId)
                .ToList();
            chart.pieChartData = new PieChartData()
            {
                Attended = studentJoinLesson.Count(x => x.Status == "Có mặt"),
                Late = studentJoinLesson.Count(x => x.Status == "Đi trễ"),
                Absent = studentJoinLesson.Count(x => x.Status == "Có phép" || x.Status == "Không phép"),
            };

            var groupedData = studentJoinLesson
                .GroupBy(x => x.Lesson.CourseClass.Subject?.Name) // Nhóm theo tên môn học
                .Select(group => new BlockChartDetail
                {
                    Subject = group.Key,
                    Attended = group.Count(x => x.Status == "Có mặt"),
                    Late = group.Count(x => x.Status == "Đi trễ"),
                    Absent = group.Count(x => x.Status == "Có phép" || x.Status == "Không phép")
                })
                .ToList();
            chart.blockChartData = new BlockChartData()
            {
                data = groupedData
            };

            return Json(chart);
        }

        public IActionResult Index()
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var student = _context.Students.AsNoTracking().FirstOrDefault(x => x.UserId == userId);
            if(student == null)
            {
                return NotFound();
            }
            Semester? current = StudyYearService.GetCurrentSemester(_context);
            ViewBag.StudyYearDetails = new SelectList(
                _context.StudyYearDetails.AsNoTracking()
                .Select(x => new SelectListItem()
                {
                    Value = x.Id.ToString(),
                    Text = x.StartYear.ToString() + " - " + x.EndYear.ToString()
                })
                .ToList(),
                "Value",
                "Text",
                current?.SchoolYearDetailId
            );
            ViewBag.Student = student;
            return View(student?.Curriculum);
        }
    }
}
