using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagementASP.Models;

namespace StudentManagementASP.Controllers
{
    [Authorize]
    public class CourseController : Controller
    {
        private readonly StudentManagementContext _context;

        public CourseController(StudentManagementContext context)
        {
            _context = context;
        }

        public IActionResult List()
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
