using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentManagementASP.Models;
using StudentManagementASP.ViewModels;
using System.Security.Claims;

namespace StudentManagementASP.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin,manager")]
    public class SubjectController : Controller
    {
        private readonly StudentManagementContext _context;

        public SubjectController(StudentManagementContext context)
        {
            _context = context;
        }

        public IActionResult Index(int? deptId, int page = 1)
        {
            int pageSize = 10;

            var query = _context.Subjects.AsNoTracking();

            if (deptId.HasValue)
            {
                query = query.Where(x => x.DeptId == deptId.Value);
            }

            var total = query.Count();

            var subjects = query
                .Skip((page - 1) * pageSize)  // Skip previous pages
                .Take(pageSize)              // Take the current page size
                .ToList();

            var model = new SubjectListViewModel
            {
                Subjects = subjects,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)total / pageSize)
            };

            ViewBag.Depts = _context.Departments.AsNoTracking().ToList();

            return View(model);
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

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Depts = _context.Departments.AsNoTracking().Select(x => new SelectListItem()
            {
                Value = x.Id.ToString(),
                Text = x.Code + " - " + x.Name,
            }).ToList();
            return View();
        }

        [HttpPost]
        public IActionResult Create(Subject model)
        {
            try
            {
                _context.Subjects.Add(model);
                _context.SaveChanges();
                return RedirectToAction("Details", "Subject", new {area="Admin", id = model.Id});
            }
            catch (Exception ex)
            {
                ViewBag.Depts = _context.Departments.AsNoTracking().Select(x => new SelectListItem()
                {
                    Value = x.Id.ToString(),
                    Text = x.Code + " - " + x.Name,
                }).ToList();
                ViewBag.Alert = ex.ToString();
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            var model = _context.Subjects.Find(id);
            if(model == null)
            {
                return NotFound();
            }
            return View(model);
        }


        [HttpGet]
        public IActionResult Delete(int id)
        {
            var model = _context.Subjects.Find(id);
            if (model == null)
            {
                return NotFound();
            }
            return View(model);
        }
        [HttpPost]
        public IActionResult DeleteConfirmed(int id)
        {
            var model = _context.Subjects.Find(id);
            if (model == null)
            {
                return NotFound();
            }
            try
            {
                _context.Subjects.Remove(model);
                _context.SaveChanges();
                TempData["Success"] = "Success";
                return RedirectToAction("Index", "Subject", new { area = "Admin" });
            }
            catch (Exception ex)
            {
                ViewBag.Alert = ex.ToString();
                return RedirectToAction("Delete", "Subject", new { area = "Admin", id = id });
            }
        }

        public IActionResult Edit(int id)
        {
            var model = _context.Subjects.Find(id);
            if (model == null)
            {
                return NotFound();
            }
            ViewBag.Depts = _context.Departments.AsNoTracking().Select(x => new SelectListItem()
            {
                Value = x.Id.ToString(),
                Text = x.Code + " - " + x.Name,
            }).ToList();
            return View(model);
        }


        [HttpPost]
        public IActionResult Edit(Subject model)
        {
            try
            {
                _context.Subjects.Update(model);
                _context.SaveChanges();
                return RedirectToAction("Details", "Subject", new { area = "Admin", id = model.Id });
            }
            catch (Exception ex)
            {
                ViewBag.Depts = _context.Departments.AsNoTracking().Select(x => new SelectListItem()
                {
                    Value = x.Id.ToString(),
                    Text = x.Code + " - " + x.Name,
                }).ToList();
                ViewBag.Alert = ex.ToString();
                return View(model);
            }
        }
    }
}
