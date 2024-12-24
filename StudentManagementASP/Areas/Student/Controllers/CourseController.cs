using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentManagementASP.Models;
using StudentManagementASP.Services;
using System.Security.Claims;

namespace StudentManagementASP.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "student")]
    public class CourseController : Controller
    {
        private readonly StudentManagementContext _context;

        public CourseController(StudentManagementContext context)
        {
            _context = context;
        }

        public IActionResult List()
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var student = _context.Students.AsNoTracking().FirstOrDefault(x => x.UserId == userId);
            if (student == null)
            {
                return NotFound();
            }
            var curriculum = _context.Curricula.AsNoTracking()
                .FirstOrDefault(x => x.Id == student.CurriculumId);

            ViewBag.ListSemester = _context.Semesters.AsNoTracking()
                .Where(x => x.SchoolYearDetailId >= curriculum.StudyYear.StartYearId && x.SchoolYearDetailId <= curriculum.StudyYear.EndYearId)
                .ToList();

            ViewBag.CourseClass = _context.StudentJoinClasses.AsNoTracking()
                .Where(sjc => sjc.StudentId == student.Id)
                .Include(sjc => sjc.CourseClass) // Bao gồm thông tin CourseClass
                .Select(sjc => sjc.CourseClass) // Chỉ lấy thông tin Course
                .ToList();


            return View(curriculum);
        }


        public IActionResult CourseInfo(int id)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var student = _context.Students.AsNoTracking().FirstOrDefault(x => x.UserId == userId);
            var courseClass = _context.CourseClasses.AsNoTracking().SingleOrDefault(x => x.Id == id);
            if (student == null || courseClass == null)
            {
                return NotFound();
            }
            if (courseClass == null) { return NotFound(); }
            ViewBag.StudentJoined = _context.StudentJoinLessons.AsNoTracking()
                .Include(x => x.Lesson)
                .Where(x => x.Lesson.CourseClassId == id && x.StudentId == student.Id)
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
            var student = _context.Students.AsNoTracking().FirstOrDefault(x => x.UserId == userId);
            if (student == null)
            {
                return NotFound();
            }
            DateOnly sd = new DateOnly(startDate.Year, startDate.Month, startDate.Day);
            DateOnly ed = new DateOnly(endDate.Year, endDate.Month, endDate.Day);
            var lessons = _context.Lessons.AsNoTracking()
                .Include(x => x.CourseClass)
                    .ThenInclude(x => x.StudentJoinClasses)
                .Where(x => x.CourseClass.StudentJoinClasses.Any(x => x.StudentId == student.Id)
                    && x.Date >= sd && x.Date <= ed
                )
                .OrderBy(x => x.Date)
                .ToList();
            return PartialView(lessons);
        }




    }
}
