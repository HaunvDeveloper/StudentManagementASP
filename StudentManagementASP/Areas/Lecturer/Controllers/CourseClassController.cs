using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagementASP.Models;
using System.Security.Claims;
using StudentManagementASP.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using OfficeOpenXml.Style;
using OfficeOpenXml;

namespace StudentManagementASP.Areas.Lecturer.Controllers
{
    [Area("Lecturer")]
    [Authorize(Roles = "lecturer")]
    public class CourseClassController : Controller
    {
        private readonly StudentManagementContext _context;

        public CourseClassController(StudentManagementContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            Semester? current = StudyYearService.GetCurrentSemester(_context);
            ViewBag.StudyYearDetails = new SelectList(
                _context.StudyYearDetails.AsNoTracking()
                .Select(x => new SelectListItem()
                {
                    Value = x.Id.ToString(),
                    Text = x.StartYear.ToString() + " - " + x.EndYear.ToString()
                })
                .ToList(),
                "Value",
                "Text",
                current?.SchoolYearDetailId
            );
            return View();
        }
    
        public IActionResult _GetListClass(int semesterId)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var lecturer = _context.Lecturers.AsNoTracking().Single(x => x.UserId == userId);
            var listClass = _context.CourseClasses.AsNoTracking()
                .Where(x => x.LecturerId == lecturer.Id && x.SemesterId == semesterId)
                .ToList();
            return PartialView(listClass);
        }

        public IActionResult Schedules()
        {
			
            Semester? current = StudyYearService.GetCurrentSemester(_context);
            ViewBag.StudyYearDetails = new SelectList(
                _context.StudyYearDetails.AsNoTracking()
                .Select(x => new SelectListItem()
                {
                    Value = x.Id.ToString(),
                    Text = x.StartYear.ToString() + " - " + x.EndYear.ToString()
                })
                .ToList(),
                "Value",
                "Text",
                current?.SchoolYearDetailId
            );
           
			return View();
        }
        public IActionResult _GetSchedules(DateTime startDate, DateTime endDate)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var lecturer = _context.Lecturers.AsNoTracking().Single(x => x.UserId == userId);
            DateOnly sd = new DateOnly(startDate.Year, startDate.Month, startDate.Day);
            DateOnly ed = new DateOnly(endDate.Year, endDate.Month, endDate.Day);
            var lessons = (from lesson in _context.Lessons.AsNoTracking()
                           join courseClass in _context.CourseClasses.AsNoTracking()
                           on lesson.CourseClassId equals courseClass.Id
                           where courseClass.LecturerId == lecturer.Id
                           && lesson.Date >= sd
                           && lesson.Date <= ed
                           orderby lesson.Date
                           select lesson)
              .ToList();

            return PartialView(lessons);
        }


        public IActionResult ViewStudentList(int id)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var lecturer = _context.Lecturers.AsNoTracking().Single(x => x.UserId == userId);

            var courseClass = _context.CourseClasses.AsNoTracking()
                .Include(x => x.StudentJoinClasses)
                .SingleOrDefault(x => x.Id == id && x.LecturerId == lecturer.Id);

            if (courseClass == null)
            {
                return NotFound();
            }
            ViewBag.LessonJoined = _context.StudentJoinLessons
                .AsNoTracking()
                .GroupJoin(_context.Lessons, sj => sj.LessonId, l => l.Id, (sj, l) => new { sj, l })
                .SelectMany(x => x.l.DefaultIfEmpty(), (x, lJoined) => new { x.sj, lJoined })
                .Where(x => x.lJoined.CourseClassId == courseClass.Id)
                .Select(x => x.sj)
                .ToList();
            return View(courseClass);
        }

        public IActionResult ExportStudentList(int id)
        {
            // Đường dẫn đến tệp Excel trong wwwroot/Data/
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Data", "DanhSachSVLHP.xlsx");

            // Kiểm tra tệp có tồn tại
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("Tệp không tồn tại.");
            }

            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var lecturer = _context.Lecturers.AsNoTracking().Single(x => x.UserId == userId);
            var courseClass = _context.CourseClasses.AsNoTracking().SingleOrDefault(x => x.Id == id && x.LecturerId == lecturer.Id);
            if (courseClass == null)
            {
                return NotFound();
            }

            var list = _context.StudentJoinClasses
                .Include(x => x.Student)
                .Where(x => x.CourseClassId == id)
                .ToList();
            var lessonJoins = _context.StudentJoinLessons
                .AsNoTracking()
                .GroupJoin(_context.Lessons, sj => sj.LessonId, l => l.Id, (sj, l) => new { sj, l })
                .SelectMany(x => x.l.DefaultIfEmpty(), (x, lJoined) => new { x.sj, lJoined })
                .Where(x => x.lJoined.CourseClassId == courseClass.Id)
                .Select(x => x.sj)
                .ToList();
            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[0]; // Sử dụng worksheet đầu tiên

                worksheet.Cells[$"D4"].Value = courseClass?.Semester.Name;
                worksheet.Cells[$"I4"].Value = courseClass?.Semester.SchoolYearDetail.StartYear + " - " + courseClass?.Semester.SchoolYearDetail.StartYear;

                worksheet.Cells[$"E5"].Value = courseClass?.Subject?.Code + " - " + courseClass?.Name;
                worksheet.Cells[$"W5"].Value = courseClass?.Code;
                worksheet.Cells[$"A6"].Value = $"Thời gian học: Bắt đầu: {courseClass?.StartDate.ToString("dd/MM/yyyy")} - Kết thúc: {courseClass?.EndDate.ToString("dd/MM/yyyy")}";
                worksheet.Cells[$"B7"].Value = courseClass?.WeakDays;


                // Xóa dữ liệu cũ (nếu cần)
                var startRow = 10; // Giả sử dữ liệu bắt đầu từ dòng 5

                // Điền dữ liệu mới
                var currentRow = startRow;
                int stt = 1;
                foreach (var item in list)
                {
                    string[] nameParts = item.Student.FullName.Trim().Split(' ');
                    string ho = string.Join(" ", nameParts, 0, nameParts.Length - 1);
                    string ten = nameParts[nameParts.Length - 1];
                    worksheet.Cells[$"A{currentRow}"].Value = stt++; // STT
                    worksheet.Cells[$"C{currentRow}"].Value = item.Student.Id;          // Mã số sinh viên
                    worksheet.Cells[$"F{currentRow}"].Value = item.Student.StudentClass?.Code;    // Họ lót
                    worksheet.Cells[$"H{currentRow}"].Value = ho;   // Tên
                    worksheet.Cells[$"M{currentRow}"].Value = ten; // Ngày sinh
                    worksheet.Cells[$"Q{currentRow}"].Value = item.Student.DayOfBirth.ToString("dd/MM/yyyy");
                    int coMat = lessonJoins.Count(x => x.StudentId == item.Student.Id && x.Status == "Có mặt");
                    int diTre = lessonJoins.Count(x => x.StudentId == item.Student.Id && x.Status == "Đi trễ");
                    int vang = lessonJoins.Count(x => x.StudentId == item.Student.Id && x.Status == "Vắng");
                    worksheet.Cells[$"U{currentRow}"].Value = coMat;
                    worksheet.Cells[$"X{currentRow}"].Value = diTre;
                    worksheet.Cells[$"AB{currentRow}"].Value = vang;
                    currentRow++;
                }
                // Thiết lập đường viền cho vùng ô A1:M10
                var range = worksheet.Cells[$"A9:AD{currentRow - 1}"];
                range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                // Điều chỉnh kích thước cột

                // Trả lại tệp Excel đã chỉnh sửa
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                var fileName = "DanhSachSinhVien_ChinhSua.xlsx";
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return File(stream, contentType, fileName);
            }
        }
    
        public IActionResult ListTime()
        {
            Semester? current = StudyYearService.GetCurrentSemester(_context);
            ViewBag.StudyYearDetails = new SelectList(
                _context.StudyYearDetails.AsNoTracking()
                .Select(x => new SelectListItem()
                {
                    Value = x.Id.ToString(),
                    Text = x.StartYear.ToString() + " - " + x.EndYear.ToString()
                })
                .ToList(),
                "Value",
                "Text",
                current?.SchoolYearDetailId
            );
            return View();
        }

        public IActionResult _GetListTime(int semesterId)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var lecturer = _context.Lecturers.AsNoTracking().Single(x => x.UserId == userId);
            var listClass = _context.CourseClasses.AsNoTracking()
                .Where(x => x.LecturerId == lecturer.Id && x.SemesterId == semesterId)
                .ToList();
            return PartialView(listClass);
        }

        public IActionResult _GetListLesson(int courseClassId)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var lecturer = _context.Lecturers.AsNoTracking().Single(x => x.UserId == userId);
            var courseClass = _context.CourseClasses.AsNoTracking()
                .Include(x => x.Lessons)
                .SingleOrDefault(x => x.Id == courseClassId && x.LecturerId == lecturer.Id);
            if(courseClass == null)
            {
                return NotFound();
            }
            return PartialView(courseClass);
        }
    }
}
