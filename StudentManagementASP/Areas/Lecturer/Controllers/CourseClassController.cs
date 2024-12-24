using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagementASP.Models;
using System.Security.Claims;
using StudentManagementASP.Services;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace StudentManagementASP.Areas.Lecturer.Controllers
{
    [Area("Lecturer")]
    [Authorize(Roles = "lecturer")]
    public class CourseClassController : Controller
    {
        private readonly StudentManagementContext _context;

        public CourseClassController(StudentManagementContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var lecturer = _context.Lecturers.AsNoTracking()
                .FirstOrDefault(x => x.UserId == userId);
            if (lecturer == null)
            {
                return NotFound();
            }
            var courseClass = _context.CourseClasses
                .AsNoTracking()
                .Where(x => x.LecturerId == lecturer.Id)
                .ToList();

            return View(courseClass);
        }
    
        public IActionResult Schedules()
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
        public IActionResult _GetSchedules(DateTime startDate, DateTime endDate)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var lecturer = _context.Lecturers.AsNoTracking().Single(x => x.UserId == userId);
            DateOnly sd = new DateOnly(startDate.Year, startDate.Month, startDate.Day);
            DateOnly ed = new DateOnly(endDate.Year, endDate.Month, endDate.Day);
            var lessons = (from lesson in _context.Lessons.AsNoTracking()
                           join courseClass in _context.CourseClasses.AsNoTracking()
                           on lesson.CourseClassId equals courseClass.Id
                           where courseClass.LecturerId == lecturer.Id
                           && lesson.Date >= sd
                           && lesson.Date <= ed
                           orderby lesson.Date
                           select lesson)
              .ToList();

            return PartialView(lessons);
        }
    
    
        public IActionResult ListRegist()
        {
            ViewBag.StudyYearDetails = _context.StudyYearDetails.AsNoTracking()
                .Select(x => new SelectListItem()
                {
                    Value = x.Id.ToString(),
                    Text = x.StartYear.ToString() + " - " + x.EndYear.ToString()
                })
                .ToList();
            return View(); 
        }
    }
}
