using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentManagementASP.Models;
using StudentManagementASP.ViewModels;

namespace StudentManagementASP.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles ="admin")]
    public class MajorController : Controller
    {
        private readonly StudentManagementContext _context;

        public MajorController(StudentManagementContext context)
        {
            _context = context;
        }

        public IActionResult Index(int? deptId, int page = 1)
        {
            int pageSize = 10;

            var query = _context.Majors.AsNoTracking();

            if (deptId.HasValue)
            {
                query = query.Where(x => x.DeptId == deptId.Value);
            }

            var total = query.Count();

            var subjects = query
                .Skip((page - 1) * pageSize)  // Skip previous pages
                .Take(pageSize)              // Take the current page size
                .ToList();

            var model = new MajorListViewModel
            {
                Majors = subjects,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)total / pageSize)
            };

            ViewBag.Depts = _context.Departments.AsNoTracking().ToList();

            return View(model);
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
        public IActionResult Create(Major model)
        {
            try
            {
                _context.Majors.Add(model);
                _context.SaveChanges();
                return RedirectToAction("Details", "Major", new { area = "Admin", id = model.Id });
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
            var model = _context.Majors.Find(id);
            if (model == null)
            {
                return NotFound();
            }
            return View(model);
        }


        [HttpGet]
        public IActionResult Delete(int id)
        {
            var model = _context.Majors.Find(id);
            if (model == null)
            {
                return NotFound();
            }
            return View(model);
        }
        [HttpPost]
        public IActionResult DeleteConfirmed(int id)
        {
            var model = _context.Majors.Find(id);
            if (model == null)
            {
                return NotFound();
            }
            try
            {
                _context.Majors.Remove(model);
                _context.SaveChanges();
                TempData["Success"] = "Success";
                return RedirectToAction("Index", "Major", new { area = "Admin" });
            }
            catch (Exception ex)
            {
                ViewBag.Alert = ex.ToString();
                return RedirectToAction("Delete", "Major", new { area = "Admin", id = id });
            }
        }

        public IActionResult Edit(int id)
        {
            var model = _context.Majors.Find(id);
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
        public IActionResult Edit(Major model)
        {
            try
            {
                _context.Majors.Update(model);
                _context.SaveChanges();
                return RedirectToAction("Details", "Major", new { area = "Admin", id = model.Id });
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
