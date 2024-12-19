using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentManagementASP.Models;
using System.Security.Claims;

namespace StudentManagementASP.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin,manager")]
    public class CourseController : Controller
    {
        private readonly StudentManagementContext _context;

        public CourseController(StudentManagementContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            ViewBag.StudyYear = new SelectList(_context.StudyYears.AsNoTracking().ToList(), "Id", "Number");
            return View();
        }

        public IActionResult _GetListCurriculum(int? CurriculumId)
        {
            var curriculum = _context.Curricula.AsNoTracking()
                .Where(x => x.Id == CurriculumId)
                .Include(x => x.Courses)
                .Include(x => x.StudyYear)
                .FirstOrDefault();

            if(curriculum == null)
            {
                return NotFound();
            }
            ViewBag.ListYear = _context.StudyYearDetails.AsNoTracking()
                .Where(x => x.Id >= curriculum.StudyYear.StartYearId && x.Id <= curriculum.StudyYear.EndYearId)
                .ToList();
            return PartialView(curriculum);
        }
    }
}
