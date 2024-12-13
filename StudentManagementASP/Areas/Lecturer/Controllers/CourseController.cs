using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagementASP.Models;
using System.Security.Claims;

namespace StudentManagementASP.Areas.Lecturer.Controllers
{
    [Area("Lecturer")]
    [Authorize(Roles = "lecturer")]
    public class CourseController : Controller
    {
        private readonly StudentManagementContext _context;

        public CourseController(StudentManagementContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var lecturer = _context.Lecturers.AsNoTracking().FirstOrDefault(x => x.UserId == userId);
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
    }
}
