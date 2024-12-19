using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using StudentManagementASP.Models;
using StudentManagementASP.Services;
using StudentManagementASP.ViewModels;

namespace StudentManagementASP.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin,manager")]
    public class CourseClassController : Controller
    {
        private readonly StudentManagementContext _context;

        public CourseClassController(StudentManagementContext context)
        {
            _context = context;
            ExcelPackage.LicenseContext = LicenseContext.Commercial;
        }

        public IActionResult Index()
        {
            ViewBag.StudyYearDetails = _context.StudyYearDetails.AsNoTracking()
                .Select(x => new SelectListItem()
                {
                    Value = x.Id.ToString(),
                    Text = x.StartYear.ToString() + " - " + x.EndYear.ToString()
                })
                .ToList();
            ViewBag.Depts = new SelectList(_context.Departments.AsNoTracking().ToList(), "Id", "Name");
            return View();
        }

        public IActionResult _GetListClass(int? deptId, int? yearDetailId, int? semesterId)
        {
            var query = _context.CourseClasses.AsNoTracking();
            if (semesterId.HasValue)
            {
                query = query.Where(x => x.SemesterId == semesterId);
            }
            else if (yearDetailId.HasValue)
            {
                query = query
                    .GroupJoin(
                        _context.Semesters,
                        cs => cs.SemesterId,
                        ses => ses.Id,
                        (cs, ses) => new { cs, ses }
                    )
                    .SelectMany(
                        x => x.ses.DefaultIfEmpty(),
                        (x, sesJoined) => new { x.cs, sesJoined }
                    )
                    .Where(x => x.sesJoined != null && x.sesJoined.SchoolYearDetailId == yearDetailId)
                    .Select(x => x.cs);

            }
            if (deptId.HasValue)
            {
                query = query.GroupJoin(
                            _context.Subjects,
                            x => x.SubjectId,
                            subj => subj.Id,
                            (x, subj) => new { x, subj }
                        )
                        .SelectMany(
                            x => x.subj.DefaultIfEmpty(),
                            (x, subjJoined) => new { x.x, subjJoined }
                        )
                        .Where(x => x.subjJoined != null && x.subjJoined.DeptId == deptId)
                        .Select(x => x.x);
            }
            var list = query.ToList();
            return PartialView(list);
        }

        public IActionResult GetClassBySubject(int subjectId, int semesterId)
        {
            var subject = _context.Subjects.AsNoTracking().FirstOrDefault(x => x.Id == subjectId);
            if (subject == null)
            {
                return NotFound();
            }
            ViewBag.Subject = subject;
            var list = _context.CourseClasses.AsNoTracking().Where(x => x.SubjectId == subjectId).ToList();
            return View(list);
        }

        public IActionResult Create()
        {
            ViewBag.StudyYearDetails = _context.StudyYearDetails.AsNoTracking()
                .Select(x => new SelectListItem()
                {
                    Value = x.Id.ToString(),
                    Text = x.StartYear.ToString() + " - " + x.EndYear.ToString()
                })
                .ToList();
            ViewBag.StudentClasses = new SelectList(_context.StudentClasses.AsNoTracking().ToList(), "Id", "Code");
            ViewBag.Subjects = new SelectList(_context.Subjects.AsNoTracking().ToList(), "Id", "Name");
            ViewBag.Lessons = _context.LessonInfos.AsNoTracking().Select(x => new SelectListItem()
            {
                Value = x.Id.ToString(),
                Text = $"Tiết {x.Id} | {x.StartTime.ToShortTimeString()} - {x.EndTime.ToShortTimeString()}"
            }).ToList();
            ViewBag.Rooms = _context.Rooms.AsNoTracking().Select(x => new SelectListItem()
            {
                Value = x.Id.ToString(),
                Text = x.Name + " - " + x.Address
            });
            ViewBag.WeekDays = new SelectList(WeekDayViewModel.GetAll(), "Id", "Name");
            
            ViewBag.Lecturers = _context.Lecturers.AsNoTracking().Select(x => new SelectListItem()
            {
                Value = x.Id.ToString(),
                Text = x.Id + " - " + x.FullName
            });
            return View();
        }

        [HttpPost]
        public IActionResult Create(CourseClass model, List<WeekDayInputViewModel> weekDays)
        {
            try
            {
                model.CurrentQuantity = 0;
                List<string> tkb = new List<string>();
                foreach(var weekDay in weekDays)
                {
                    DateTime date = WeekDayService.FindNearestWeekDay(model.StartDate, weekDay.WeekDayId);
                    var room = _context.Rooms.AsNoTracking().SingleOrDefault(x => x.Id == weekDay.RoomId);
                    tkb.Add($"{new WeekDayViewModel(weekDay.WeekDayId).ToString()}, Tiết {weekDay.StartLessonId} - {weekDay.EndLessonId}, Phòng {room.Name}");
                    while(date <= model.EndDate)
                    {
                        Lesson lesson = new Lesson()
                        {
                            Date = new DateOnly(date.Year, date.Month, date.Day),
                            StartLesson = weekDay.StartLessonId,
                            EndLesson = weekDay.EndLessonId,
                            RoomId = weekDay.RoomId
                        };
                        model.Lessons.Add(lesson);
                        date = date.AddDays(7);
                    }
                }
                model.WeakDays = string.Join(" || ", tkb);
                _context.CourseClasses.Add(model);
                _context.SaveChanges();
                return Json(new { success = true, redirect = Url.Action("Index") });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });

            }
        }

        public IActionResult CreateWithList()
        {
            ViewBag.StudyYearDetails = _context.StudyYearDetails.AsNoTracking()
                .Select(x => new SelectListItem()
                {
                    Value = x.Id.ToString(),
                    Text = x.StartYear.ToString() + " - " + x.EndYear.ToString()
                })
                .ToList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateWithList(IFormFile file, int SemesterId)
        {
            if (file == null || file.Length == 0)
            {
                return Json(new { success = false, error = "File không hợp lệ!!!" });
            }

            try
            {
                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);

                using var package = new OfficeOpenXml.ExcelPackage(stream);
                var worksheet = package.Workbook.Worksheets[0];
                var rowCount = worksheet.Dimension.Rows;

                var courseClasses = new List<CourseClass>();
                var lessonInfos = _context.LessonInfos.AsNoTracking().ToList();
                var rooms = _context.Rooms.AsNoTracking().ToList();
                for (int row = 8; row <= rowCount; row++)
                {
                    string subjectCode = worksheet.Cells[row, 2].Text.Trim();
                    if (string.IsNullOrEmpty(subjectCode))
                    {
                        continue;
                    }
                    string? studentClassCode = string.IsNullOrEmpty(worksheet.Cells[row, 6].Text.Trim())
                            ? null
                            : worksheet.Cells[row, 6].Text.Trim();
                    int subjectId = _context.Subjects.AsNoTracking().Where(x => x.Code == subjectCode).Select(x => x.Id).FirstOrDefault();
                    int? studentClassId = null;
                    if (studentClassCode != null)
                    {
                        studentClassId = _context.StudentClasses.AsNoTracking().Where(x => x.Code == studentClassCode).Select(x => x.Id).FirstOrDefault();
                    }

                    string roomCode = worksheet.Cells[row, 8].Text.Trim();
                    int roomId = rooms.Where(x => x.Code == roomCode).Select(x => x.Id).FirstOrDefault();
                    var courseClass = new CourseClass
                    {
                        Code = worksheet.Cells[row, 3].Text.Trim(),
                        Name = worksheet.Cells[row, 4].Text.Trim(),
                        SemesterId = SemesterId,
                        StartDate = DateTime.Parse(worksheet.Cells[row, 9].Text.Trim()),
                        EndDate = DateTime.Parse(worksheet.Cells[row, 10].Text.Trim()),
                        MaxQuantity = int.Parse(worksheet.Cells[row, 7].Text.Trim()),
                        CurrentQuantity = 0,
                        CourseId = null,
                        LecturerId = int.Parse(worksheet.Cells[row, 11].Text.Trim()),
                        SubjectId = subjectId,
                        StudentClassId = studentClassId,
                        DefaultRoomId = roomId
                    };
                    List<string> tkb = new List<string>();
                    int soBuoi = int.Parse(worksheet.Cells[row, 13].Text.Trim());
                    for (int i = 1; i <= soBuoi; i++)
                    {
                        int cr = 10 + (i * 4);
                        try
                        {

                            var weekDay = new WeekDayInputViewModel()
                            {
                                WeekDayId = int.Parse(worksheet.Cells[row, cr].Text.Trim()),
                                StartLessonId = int.Parse(worksheet.Cells[row, cr + 1].Text.Trim()),
                                EndLessonId = int.Parse(worksheet.Cells[row, cr + 2].Text.Trim()),
                                RoomId = rooms.FirstOrDefault(x => x.Code == worksheet.Cells[row, cr + 3].Text.Trim())?.Id ?? 0,
                            };
                        

                            DateTime date = WeekDayService.FindNearestWeekDay(courseClass.StartDate, weekDay.WeekDayId);
                            var room = _context.Rooms.AsNoTracking().SingleOrDefault(x => x.Id == weekDay.RoomId);
                            tkb.Add($"{new WeekDayViewModel(weekDay.WeekDayId).ToString()}, Tiết {weekDay.StartLessonId} - {weekDay.EndLessonId}, Phòng {room.Name}");
                            while (date <= courseClass.EndDate)
                            {
                                Lesson lesson = new Lesson()
                                {
                                    Date = new DateOnly(date.Year, date.Month, date.Day),
                                    StartLesson = weekDay.StartLessonId,
                                    EndLesson = weekDay.EndLessonId,
                                    RoomId = weekDay.RoomId
                                };
                                courseClass.Lessons.Add(lesson);
                                date = date.AddDays(7);
                            }
                        }
                        catch { break; }
                    }
                    courseClass.WeakDays = string.Join(" || ", tkb);

                    courseClasses.Add(courseClass);
                }
                
                _context.CourseClasses.AddRange(courseClasses);
                await _context.SaveChangesAsync();
                return Json(new { success = true, redirect = Url.Action("Index") });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.ToString() });
            }
        }

        [HttpGet]
        public IActionResult DownloadExcelFile()
        {
            // Đường dẫn tới file trong thư mục wwwroot
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Data", "MauLHP.xlsx");

            // Kiểm tra nếu file tồn tại
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("File không tồn tại.");
            }

            // Lấy nội dung file
            var fileBytes = System.IO.File.ReadAllBytes(filePath);

            // Trả file về client
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "MauLHP.xlsx");
        }
    
    
        public IActionResult Edit(int id)
        {
            var model = _context.CourseClasses.AsNoTracking().FirstOrDefault(x => x.Id == id);
            if (model == null)
            {
                return NotFound();
            }
            ViewBag.StudyYearDetails = _context.StudyYearDetails.AsNoTracking()
                .Select(x => new SelectListItem()
                {
                    Value = x.Id.ToString(),
                    Text = x.StartYear.ToString() + " - " + x.EndYear.ToString()
                })
                .ToList();
            ViewBag.StudentClasses = new SelectList(_context.StudentClasses.AsNoTracking().ToList(), "Id", "Code");
            ViewBag.Subjects = new SelectList(_context.Subjects.AsNoTracking().ToList(), "Id", "Name");
            ViewBag.Lessons = _context.LessonInfos.AsNoTracking().Select(x => new SelectListItem()
            {
                Value = x.Id.ToString(),
                Text = $"Tiết {x.Id} | {x.StartTime.ToShortTimeString()} - {x.EndTime.ToShortTimeString()}"
            }).ToList();
            ViewBag.Rooms = _context.Rooms.AsNoTracking().Select(x => new SelectListItem()
            {
                Value = x.Id.ToString(),
                Text = x.Name + " - " + x.Address
            });
            ViewBag.Lecturers = _context.Lecturers.AsNoTracking().Select(x => new SelectListItem()
            {
                Value = x.Id.ToString(),
                Text = x.Id + " - " + x.FullName
            });
            
            return View(model);
        }



        [HttpPost]
        public IActionResult Edit(CourseClass model)
        {
            try
            {
                var oldModel = _context.CourseClasses
                    .Include(cc => cc.Lessons) // Nạp danh sách Lessons từ CSDL
                    .FirstOrDefault(cc => cc.Id == model.Id);

                if (oldModel != null)
                {
                    // Cập nhật thông tin lớp học phần
                    oldModel.Code = model.Code;
                    oldModel.Name = model.Name;
                    oldModel.LecturerId = model.LecturerId;
                    oldModel.StartDate = model.StartDate;
                    oldModel.EndDate = model.EndDate;
                    oldModel.StudentClassId = model.StudentClassId;
                    oldModel.MaxQuantity = model.MaxQuantity;

                    // Danh sách các Lesson hiện tại và mới
                    var newLessons = model.Lessons;
                    var existingLessons = oldModel.Lessons.ToList();

                    // Xóa những Lesson không còn trong danh sách mới
                    var lessonsToRemove = existingLessons
                        .Where(oldLesson => !newLessons.Any(newLesson => newLesson.Id == oldLesson.Id))
                        .ToList();

                    foreach (var lesson in lessonsToRemove)
                    {
                        _context.Lessons.Remove(lesson);
                    }

                    // Cập nhật những Lesson đã tồn tại
                    foreach (var lesson in newLessons.Where(l => l.Id != 0))
                    {
                        var existingLesson = existingLessons.FirstOrDefault(l => l.Id == lesson.Id);
                        if (existingLesson != null)
                        {
                            existingLesson.StartLesson = lesson.StartLesson;
                            existingLesson.EndLesson = lesson.EndLesson;
                            existingLesson.Date = lesson.Date;
                            existingLesson.RoomId = lesson.RoomId;
                        }
                    }

                    // Thêm mới các Lesson chưa tồn tại
                    var lessonsToAdd = newLessons.Where(newLesson => newLesson.Id == 0).ToList();
                    foreach (var lesson in lessonsToAdd)
                    {
                        oldModel.Lessons.Add(lesson);
                    }

                    // Lưu các thay đổi
                    _context.SaveChanges();
                }

                return Json(new { success = true, redirect = Url.Action("Index") });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }




        public IActionResult Delete(int id)
        {
            var model = _context.CourseClasses.Find(id);
            return View(model);
        }

        [HttpPost]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                var model = _context.CourseClasses.Find(id);
                if (model != null)
                {
                    _context.CourseClasses.Remove(model);
                    _context.SaveChanges();
                }
                return Json(new { success = true, redirect = Url.Action("Index") });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error=ex.Message });

            }
        }
    }
}
