using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using StudentManagementASP.Models;
using StudentManagementASP.ViewModels;
using System.Security.Cryptography;

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


        public IActionResult _GetList(int? StudyYearId, int? DeptId, int? SpecializationId, string keyword, int p = 1, int s = 20)
        {
            var query = _context.Students.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.ToLower();
                query = query.Where(ex => ex.FullName.ToLower().Contains(keyword) || ex.Id.ToLower().Contains(keyword));
            }

            if (StudyYearId.HasValue)
            {
                query = query.Where(ex => ex.Curriculum.StudyYearId == StudyYearId.Value);
            }

            if (DeptId.HasValue)
            {
                query = query.Where(ex => ex.DeptId == DeptId.Value);
            }

            if (SpecializationId.HasValue)
            {
                query = query.Where(ex => ex.MajorId == SpecializationId.Value);
            }

            var totalRecords = query.Count();

            var listData = query
                .Include(ex => ex.Curriculum)
                .Include(ex => ex.Major)
                .Include(ex => ex.Dept)
                .AsNoTracking()
                .OrderByDescending(ex => ex.Id)
                .Skip((p - 1) * s)
                .Take(s)
                .ToList();


            ViewBag.CurrentPage = p;
            ViewBag.PageSize = s;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalRecords / s);
            
            return PartialView(listData);
        }
    }
}
