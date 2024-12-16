using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagementASP.Models;

namespace StudentManagementASP.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin,manager")]
    public class CourseClassController : Controller
    {
        private readonly StudentManagementContext _context;

        public CourseClassController(StudentManagementContext context)
        {
            _context = context;
        }
        public IActionResult List(int? courseId)
        {
            var course = _context.Courses.AsNoTracking().FirstOrDefault(x => x.Id == courseId);
            if(course == null)
            {
                return NotFound();
            }
            ViewBag.Course = course;
            var list = _context.CourseClasses.AsNoTracking().Where(x => x.CourseId == courseId).ToList();
            return View(list);
        }
    }
}
