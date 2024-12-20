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
    public class DepartmentController : Controller
    {
        private readonly StudentManagementContext _context;

        public DepartmentController(StudentManagementContext context)
        {
            _context = context;
        }

        public IActionResult Index(int page = 1)
        {
            int pageSize = 10;

            var query = _context.Departments.AsNoTracking();

            var total = query.Count();

            var subjects = query
                .Skip((page - 1) * pageSize)  // Skip previous pages
                .Take(pageSize)              // Take the current page size
                .ToList();

            var model = new DepartmentViewModel
            {
                Departments = subjects,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)total / pageSize)
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Department model)
        {
            try
            {
                model.DateFound = DateTime.Now;
                _context.Departments.Add(model);
                _context.SaveChanges();
                TempData["Success"] = "Tạo thành công";
                return RedirectToAction("Index", "Department", new { area = "Admin", id = model.Id });
            }
            catch (Exception ex)
            {
                ViewBag.Alert = ex.ToString();
                return View(model);
            }
        }


        [HttpGet]
        public IActionResult Delete(int id)
        {
            var model = _context.Departments.Find(id);
            if (model == null)
            {
                return NotFound();
            }
            return View(model);
        }
        [HttpPost]
        public IActionResult DeleteConfirmed(int id)
        {
            var model = _context.Departments.Find(id);
            if (model == null)
            {
                return NotFound();
            }
            try
            {
                _context.Departments.Remove(model);
                _context.SaveChanges();
                TempData["Success"] = "Xóa thành công";
                return RedirectToAction("Index", "Department", new { area = "Admin" });
            }
            catch (Exception ex)
            {
                ViewBag.Alert = ex.ToString();
                return RedirectToAction("Delete", "Department", new { area = "Admin", id = id });
            }
        }

        public IActionResult Edit(int id)
        {
            var model = _context.Departments.Find(id);
            if (model == null)
            {
                return NotFound();
            }
            return View(model);
        }


        [HttpPost]
        public IActionResult Edit(Department model)
        {
            try
            {
                _context.Departments.Update(model);
                _context.SaveChanges();
                TempData["Success"] = "Sửa thành công";
                return RedirectToAction("Index", "Department", new { area = "Admin", id = model.Id });
            }
            catch (Exception ex)
            {
                ViewBag.Alert = ex.ToString();
                return View(model);
            }
        }
    }
}
