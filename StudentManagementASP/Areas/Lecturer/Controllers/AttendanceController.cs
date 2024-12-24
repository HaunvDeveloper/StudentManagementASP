using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentManagementASP.Models;
using StudentManagementASP.Services;
using System.Security.Claims;

namespace StudentManagementASP.Areas.Lecturer.Controllers
{
    [Area("Lecturer")]
    [Authorize(Roles = "lecturer")]
    public class AttendanceController : Controller
    {
        private readonly StudentManagementContext _context;

        public AttendanceController(StudentManagementContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
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
            return View();
        }


        public IActionResult GetListClassAPI(int semesterId)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var lecturer = _context.Lecturers.AsNoTracking().Single(x => x.UserId == userId);
            var listClass = _context.CourseClasses.AsNoTracking()
                .Where(x => x.LecturerId == lecturer.Id && x.SemesterId == semesterId)
                .ToList();
            return Json(listClass.Select(x => new
            {
                x.Id,
                Name = x.Code + " - " + x.Name,
            }).ToList());
        }

        public IActionResult GetLessonAPI(int classId)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var lecturer = _context.Lecturers.AsNoTracking().Single(x => x.UserId == userId);
            var courseClass = _context.CourseClasses.AsNoTracking().SingleOrDefault(x => x.Id ==  classId && x.LecturerId == lecturer.Id);
            if(courseClass == null)
            {
                return Json(null);
            }
            string[] dayOfWeek = ["Chủ Nhật", "Thứ Hai", "Thứ Ba", "Thứ Tư", "Thứ Năm", "Thứ Sáu", "Thứ Bảy"];
            return Json(courseClass.Lessons.Select(x => new {
                Id = x.Id,
                Name = $"{x.Date?.ToString("dd/MM/yyyy")} - {dayOfWeek[(int)x.Date?.DayOfWeek]} - {x.Room.Code}"
            }).ToList());
        }
    }
}
