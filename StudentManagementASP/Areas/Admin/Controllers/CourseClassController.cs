using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using StudentManagementASP.Models;
using StudentManagementASP.Services;
using StudentManagementASP.ViewModels;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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
    
        public IActionResult ImportStudentList(int id)
        {
            var courseClass = _context.CourseClasses.Find(id);
            if (courseClass == null)
            {
                return NotFound();
            }
            return View(courseClass);
        }

        [HttpPost]
        public async Task<IActionResult> ImportStudentList(IFormFile file, int Id)
        {
            if (file == null || file.Length == 0)
            {
                return Json(new { success = false, error = "File không hợp lệ!!!" });
            }

            var model = _context.CourseClasses.Find(Id);
            if (model == null)
            {
                return Json(new { success = false, error = "Id không tồn tại!!!" });

            }

            try
            {
                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);

                using var package = new OfficeOpenXml.ExcelPackage(stream);
                var worksheet = package.Workbook.Worksheets[0];
                var rowCount = worksheet.Dimension.Rows;
                var list = new List<StudentJoinClass>();
                
                List<string> error = new List<string>();
                int failNum = 0;
                for (int row = 5; row <= rowCount; row++)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(worksheet.Cells[row, 2].Text.Trim()))
                        {
                            break;
                        }
                        string MSSV = worksheet.Cells[row, 2].Text.Trim();
                        if (!_context.Students.AsNoTracking().Any(x => x.Id == MSSV))
                        {
                            failNum++;
                            error.Add($"{MSSV} Không tồn tại");
                            continue;
                        }
                        if (_context.StudentJoinClasses.AsNoTracking().Any(x => x.StudentId == MSSV && x.CourseClassId == model.Id))
                        {
                            failNum++;
                            error.Add($"{MSSV} đã tồn tại trong lớp học phần");
                            continue;
                        }
                        var studentJoinClass = new StudentJoinClass()
                        {
                            CourseClassId = model.Id,
                            StudentId = worksheet.Cells[row, 2].Text.Trim(),
                            DateJoin = DateTime.Now,
                        };

                        list.Add(studentJoinClass);
                    }
                    catch (Exception ex)
                    {
                        error.Add(worksheet.Cells[row, 2].Text.Trim() + " has error: " + ex.ToString());
                        continue;
                    }
                }

                _context.StudentJoinClasses.AddRange(list);
                model.CurrentQuantity = list.Count;
                await _context.SaveChangesAsync();
                if (failNum > 0)
                {
                    return Json(new { success = true, redirect = Url.Action("Index", "CourseClass", new { area = "Admin" }), message = "Has some error:" + string.Join("\n", error) });
                }
                return Json(new
                {
                    success = true,
                    redirect = Url.Action("Index", "CourseClass", new { area = "Admin" })
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.ToString() });
            }
        }



        [HttpGet]
        public IActionResult DownloadImportStudentList()
        {
            // Đường dẫn tới file trong thư mục wwwroot
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Data", "MauThemSV_LHP.xlsx");

            // Kiểm tra nếu file tồn tại
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("File không tồn tại.");
            }

            // Lấy nội dung file
            var fileBytes = System.IO.File.ReadAllBytes(filePath);

            // Trả file về client
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "MauThemSV_LHP.xlsx");
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

            // Lấy dữ liệu từ cơ sở dữ liệu
            var list = _context.StudentJoinClasses
                .Include(x => x.Student)
                .Where(x => x.CourseClassId == id)
                .ToList();
            var courseClass = _context.CourseClasses.AsNoTracking().SingleOrDefault(x => x.Id == id);
            var lessonJoins = _context.StudentJoinLessons
                .AsNoTracking()
                .GroupJoin(_context.Lessons, sj => sj.LessonId, l => l.Id, (sj, l) => new { sj, l })
                .SelectMany(x => x.l.DefaultIfEmpty(), (x, lJoined) => new {x.sj, lJoined})
                .Where(x => x.lJoined.CourseClassId == courseClass.Id)
                .Select(x => x.sj)
                .ToList();
            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[0]; // Sử dụng worksheet đầu tiên

                worksheet.Cells[$"D4"].Value = courseClass?.Semester.Name;
                worksheet.Cells[$"I4"].Value = courseClass?.Semester.SchoolYearDetail.StartYear + " - " +  courseClass?.Semester.SchoolYearDetail.StartYear;

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


        public IActionResult ViewStudentList(int id)
        {
            var courseClass = _context.CourseClasses.AsNoTracking()
                .Include(x => x.StudentJoinClasses)
                .SingleOrDefault(x => x.Id == id);
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

        [HttpPost]
        public IActionResult AddStudentIntoClass(string studentId, int courseClassId)
        {
            try
            {
                if(_context.StudentJoinClasses.Any(x => x.CourseClassId == courseClassId && x.StudentId == studentId))
                {
                    return Json(new { success = false, error = "Sinh viên đã tồn tại" });
                }
                _context.StudentJoinClasses.Add(new StudentJoinClass()
                {
                    StudentId = studentId,
                    CourseClassId = courseClassId,
                    DateJoin = DateTime.Now
                });
                _context.SaveChanges();
                return Json(new { success = true, redirect=Url.Action("ViewStudentList", "CourseClass", new {area="Admin", id=courseClassId}) });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error=ex.ToString() });
            }
        }

        [HttpPost]
        public IActionResult RemoveStudentFromClass(string studentId, int courseClassId)
        {
            try
            {
                
                var model = _context.StudentJoinClasses.FirstOrDefault(x => x.StudentId ==studentId && x.CourseClassId == courseClassId);
                if (model != null)
                {
                    _context.StudentJoinClasses.Remove(model);
                    _context.SaveChanges();
                }
                return Json(new { success = true, redirect = Url.Action("ViewStudentList", "CourseClass", new { area = "Admin", id = courseClassId }) });
            }
            catch (Exception ex)
            {
                return Json(new { success = true, error = ex.ToString() });
            }
        }

    }
}
