using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentManagementASP.Models;

namespace StudentManagementASP.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin,manager")]
    public class CurriculumController : Controller
    {
        private readonly StudentManagementContext _context;

        public CurriculumController(StudentManagementContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            ViewBag.StudyYear = new SelectList(_context.StudyYears.AsNoTracking().ToList(), "Id", "Number");
            ViewBag.Major = new SelectList(_context.Majors.AsNoTracking(), "Id", "Name");
            return View();
        }

        public IActionResult _GetList(int? yearId, int? majorId)
        {
            IQueryable<Curriculum> query = _context.Curricula.AsNoTracking();
            if (yearId.HasValue)
            {
                query = query.Where(x => x.StudyYearId == yearId.Value);
            }
            if (majorId.HasValue)
            {
                query = query.Where(x => x.MajorId == majorId.Value);
            }
            var list = query.ToList();
            return PartialView(list);
        }



        public IActionResult Create()
        {
            ViewBag.StudyYear = new SelectList(_context.StudyYears.AsNoTracking().ToList(), "Id", "Number");
            ViewBag.Major = new SelectList(_context.Majors.AsNoTracking(), "Id", "Name");
            return View();
        }

        [HttpPost]
        public IActionResult Create(Curriculum curriculum)
        {
            try
            {
                curriculum.CreatedDate = DateTime.Now;
                _context.Curricula.Add(curriculum);
                _context.SaveChanges();
                return RedirectToAction("EditCourses", "Curriculum", new { id = curriculum.Id });
            }
            catch (Exception ex)
            {
                ViewBag.StudyYear = new SelectList(_context.StudyYears.AsNoTracking().ToList(), "Id", "Number");
                ViewBag.Major = new SelectList(_context.Majors.AsNoTracking(), "Id", "Name");
                ViewBag.Alert = ex.Message ;
                return View();
            }
        }


        public IActionResult EditCourses(int id)
        {
            var curriculum = _context.Curricula.AsNoTracking()
                .Where(x => x.Id == id)
                .Include(x => x.Courses)
                .Include(x => x.StudyYear)
                .FirstOrDefault();

            if (curriculum == null)
            {
                return NotFound();
            }
            ViewBag.ListYear = _context.StudyYearDetails.AsNoTracking()
                .Where(x => x.Id >= curriculum.StudyYear.StartYearId && x.Id <= curriculum.StudyYear.EndYearId)
                .ToList();
            ViewBag.Subject = _context.Subjects.AsNoTracking().ToList();
            ViewBag.CourseType = _context.CourseTypes.AsNoTracking().ToList();
            return View(curriculum);
        }

        [HttpPost]
        public async Task<IActionResult> EditCourses(int curriculumId, List<Course> courses)
        {
            // Tìm chương trình học (Curriculum) liên quan
            var curriculum = await _context.Curricula.Include(c => c.Courses)
                                                     .FirstOrDefaultAsync(c => c.Id == curriculumId);

            if (curriculum == null)
            {
                return Json(new { success = false, message = "Curriculum not found." });
            }

            // Lấy danh sách các Course hiện tại từ DB
            var existingCourses = curriculum.Courses.ToList();

            // Tạo danh sách để thêm mới, cập nhật và xóa
            var newCourses = courses.Where(c => c.Id == 0).ToList(); // Id = 0: thêm mới
            var updatedCourses = courses.Where(c => c.Id != 0).ToList(); // Id != 0: cập nhật
            var deletedCourses = existingCourses.Where(ec => !courses.Any(c => c.Id == ec.Id)).ToList(); // Không có trong danh sách gửi lên

            // Xử lý thêm mới các khóa học
            foreach (var course in newCourses)
            {
                course.CurriculumId = curriculumId; // Gắn CurriculumId cho khóa học mới
                _context.Courses.Add(course);
            }

            // Xử lý cập nhật các khóa học
            foreach (var course in updatedCourses)
            {
                var existingCourse = existingCourses.FirstOrDefault(ec => ec.Id == course.Id);
                if (existingCourse != null)
                {
                    existingCourse.Lesson = course.Lesson;
                    existingCourse.Credits = course.Credits;
                    existingCourse.SemesterId = course.SemesterId;
                    existingCourse.TypeId = course.TypeId;
                    existingCourse.SubjectId = course.SubjectId;
                    existingCourse.Infomation = course.Infomation;
                }
            }

            // Xử lý xóa các khóa học
            foreach (var course in deletedCourses)
            {
                _context.Courses.Remove(course);
            }

            // Lưu thay đổi vào DB
            await _context.SaveChangesAsync();

            // Trả về kết quả
            return Json(new { success = true, redirect = Url.Action("Details", "Curriculum", new { id = curriculumId }) });
        }






        public IActionResult Details(int id)
        {
            var curriculum = _context.Curricula.AsNoTracking()
                .Where(x => x.Id == id)
                .Include(x => x.Courses)
                .Include(x => x.StudyYear)
                .FirstOrDefault();

            if (curriculum == null)
            {
                return NotFound();
            }
            ViewBag.ListYear = _context.StudyYearDetails.AsNoTracking()
                .Where(x => x.Id >= curriculum.StudyYear.StartYearId && x.Id <= curriculum.StudyYear.EndYearId)
                .ToList();
            return View(curriculum);
        }
    }
}
