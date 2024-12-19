using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagementASP.Models;
using System.Security.Claims;

namespace StudentManagementASP.Controllers
{
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
                .Where(x => x.Id == student.CurriculumId)
                .FirstOrDefault();
            ViewBag.ListSemester = _context.Semesters.AsNoTracking()
                .Where(x => x.SchoolYearDetailId >= curriculum.StudyYear.StartYearId && x.SchoolYearDetailId <= curriculum.StudyYear.EndYearId)
                .ToList();
            
            ViewBag.CourseClass = _context.StudentJoinClasses.AsNoTracking()
                .Where(sjc => sjc.StudentId == student.Id)
                .Include(sjc => sjc.CourseClass) // Bao gồm thông tin CourseClass
                    .ThenInclude(cc => cc.Course) // Bao gồm thông tin Course từ CourseClass
                .Select(sjc => sjc.CourseClass) // Chỉ lấy thông tin Course
                .ToList();


            return View(curriculum);
        }

        
        public IActionResult CourseInfo(int id)
        {
            var courseClass = _context.CourseClasses.AsNoTracking().SingleOrDefault(x => x.Id == id);
            if (courseClass == null) { return NotFound(); }

            return View(courseClass);
        }

        public IActionResult Schedules()
        {
            return View();
        }



        public IActionResult CreateSemesterByStudyYear(int syearid)
        {
            //var list = _context.StudyYearDetails.Where(x => x.StudyYearId == syearid).ToList();
            //foreach (var styear in list)
            //{
            //    List<Semester> semesters = new List<Semester>();
            //    int year = styear.StartYear.Year;
            //    for (int i = 1; i <= 3; i++)
            //    {
            //        var hk = new Semester
            //        {
            //            Code = "HK" + i.ToString(),
            //            Name = "Học kỳ " + i.ToString(),
            //            StartDate = DateTime.Now,
            //            EndDate = DateTime.Now,
            //            SchoolYearDetailId = styear.Id,
            //        };
            //        if (i == 1)
            //        {
            //            hk.StartDate = new DateTime(year, 8, 1);
            //            hk.EndDate = new DateTime(year, 12, 30);
            //        }
            //        else if (i == 2)
            //        {
            //            hk.StartDate = new DateTime(year + 1, 1, 1);
            //            hk.EndDate = new DateTime(year + 1, 5, 15);
            //        }
            //        else
            //        {
            //            hk.StartDate = new DateTime(year + 1, 6, 1);
            //            hk.EndDate = new DateTime(year + 1, 7, 10);
            //        }
            //        semesters.Add(hk);
            //    }
            //    _context.Semesters.AddRange(semesters);
            //    _context.SaveChanges();
            //}
            return Content("OK");
        }

        public IActionResult CreateStudyYearDetail()
        {
            //var list = _context.StudyYears.Where(x => x.Id > 1).ToList();
            //foreach (var item in list)
            //{
            //    int year = item.StartYear.Year;
            //    List<StudyYearDetail> details = new List<StudyYearDetail>();
            //    for(int i = 1; i <= 8; i++)
            //    {
            //        details.Add(new StudyYearDetail
            //        {
            //            StudyYearId = item.Id,
            //            StartYear = new DateTime(year, 1, 1),
            //            EndYear = new DateTime(year + 1, 1, 1),
            //        });
            //        year++;
            //    }
            //    _context.StudyYearDetails.AddRange(details);
            //    _context.SaveChanges();
            //}
            return Content("OK");
        }
    }
}
