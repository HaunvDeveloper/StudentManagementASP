using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentManagementASP.Models;
using StudentManagementASP.ViewModels;

namespace StudentManagementASP.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin,manager")]
    public class StudentController : Controller
    {
        private readonly StudentManagementContext _context;

        public StudentController(StudentManagementContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            ViewBag.StudyYear = new SelectList(_context.StudyYears.AsNoTracking().ToList(), "Id", "Number");
            ViewBag.Dept = new SelectList(_context.Departments.AsNoTracking(), "Id", "Name");

            return View();
        }


        public IActionResult _GetList(int? StudyYearId, int? DeptId, int? SpecializationId, string keyword)
        {
            return PartialView();
        }
    }
}
