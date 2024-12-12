using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagementASP.Models;
using StudentManagementASP.ViewModels;

namespace StudentManagementASP.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class LecturerController : Controller
    {

        private readonly StudentManagementContext _context;

        public LecturerController(StudentManagementContext context)
        {
            _context = context;
        }


        public IActionResult Index(int page = 1)
        {
            int pageSize = 10;

            var totalLecturers = _context.Lecturers.Count();
            var lecturers = _context.Lecturers
                .Skip((page - 1) * pageSize)  // Skip previous pages
                .Take(pageSize)              // Take the current page size
                .AsNoTracking()
                .Include(x => x.Dept)
                .Include(x => x.LecturerInfos)
                .ToList();

            var model = new LecturerListViewModel
            {
                Lecturers = lecturers,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalLecturers / pageSize)
            };

            return View(model);
        }

    }
}
