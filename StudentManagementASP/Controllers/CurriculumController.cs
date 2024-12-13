using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagementASP.Models;
using System.Security.Claims;

namespace StudentManagementASP.Controllers
{
    [Authorize(Roles = "student")]
    public class CurriculumController : Controller
    {
        private readonly StudentManagementContext _context;

        public CurriculumController(StudentManagementContext context)
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
                .Include(x => x.StudyYear)
                    .ThenInclude(x => x.StudyYearDetails)
                        .ThenInclude(x => x.Semesters)
                            .ThenInclude(x => x.Courses)
                .FirstOrDefault();



            return View(curriculum);
        }
    }
}
