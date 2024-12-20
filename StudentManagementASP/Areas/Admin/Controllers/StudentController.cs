using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using OfficeOpenXml;
using StudentManagementASP.Models;
using StudentManagementASP.Services;
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
            ExcelPackage.LicenseContext = LicenseContext.Commercial;
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
    
    
        public IActionResult Create()
        {
            
            return View();
        }

        public IActionResult CreateWithList()
        {
            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> CreateWithList(IFormFile file)
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
                var listStudent = new List<Student>();
                var studentClasses = _context.StudentClasses.AsNoTracking().ToList();
                var curriculums = _context.Curricula.AsNoTracking().ToList();
                var depts = _context.Departments.AsNoTracking().ToList();
                var majors = _context.Majors.AsNoTracking().ToList();
                List<string> error = new List<string>();
                int succNum = 0;
                for (int row = 6; row <= rowCount; row++)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(worksheet.Cells[row, 3].Text.Trim()))
                        {
                            break;
                        }
                        var studentClassId = studentClasses.FirstOrDefault(x => x.Code == worksheet.Cells[row, 10].Text.Trim())?.Id;

                        var curriId = curriculums.FirstOrDefault(x => x.Code == worksheet.Cells[row, 11].Text.Trim())?.Id;
                        var deptId = depts.FirstOrDefault(x => x.Code == worksheet.Cells[row, 12].Text.Trim())?.Id;
                        var majorId = majors.FirstOrDefault(x => x.Code == worksheet.Cells[row, 13].Text.Trim())?.Id;

                        if (studentClassId == null || curriId == null || deptId == null || majorId == null)
                        {
                            error.Add("Mã dữ liệu nhập không tìm thấy trong cơ sở dữ liệu");
                            continue;
                        }

                        var student = new Student
                        {
                            Id = worksheet.Cells[row, 3].Text.Trim(),
                            FullName = worksheet.Cells[row, 4].Text.Trim() + " " + worksheet.Cells[row, 5].Text.Trim(),
                            DayOfBirth = DateTime.Parse(worksheet.Cells[row, 6].Text.Trim()),
                            Email = worksheet.Cells[row, 7].Text.Trim(),
                            StudentClassId = studentClassId,
                            CurriculumId = curriId ?? 0,
                            DeptId = deptId ?? 0,
                            MajorId = majorId ?? 0,
                            PhoneNo = worksheet.Cells[row, 8].Text.Trim(),
                            Sex = worksheet.Cells[row, 9].Text.Trim(),
                            NationId = worksheet.Cells[row, 14].Text.Trim(),
                            BirthPlace = worksheet.Cells[row, 15].Text.Trim(),
                            StreetAddress = worksheet.Cells[row, 16].Text.Trim(),
                            Nation = worksheet.Cells[row, 17].Text.Trim(),
                            Religion = worksheet.Cells[row, 18].Text.Trim(),
                            User = new User()
                            {
                                Username = worksheet.Cells[row, 3].Text.Trim(),
                                Password = worksheet.Cells[row, 3].Text.Trim(),
                                FullName = worksheet.Cells[row, 4].Text.Trim() + " " + worksheet.Cells[row, 5].Text.Trim(),
                                Email = worksheet.Cells[row, 7].Text.Trim(),
                                DayOfBirth = DateTime.Parse(worksheet.Cells[row, 6].Text.Trim()),
                                IsBlock = false,
                                AuthId = 4
                            }
                        };


                        listStudent.Add(student);
                        succNum++;
                    }
                    catch (Exception ex)
                    {
                        error.Add(worksheet.Cells[row, 3].Text.Trim() + " has error: " + ex.ToString());
                        continue;
                    }
                }

                _context.Students.AddRange(listStudent);
                await _context.SaveChangesAsync();
                if (succNum > 0)
                {
                    return Json(new { success = true, redirect = Url.Action("Index", "Student", new { area = "Admin" }), message = "Has some error:" + string.Join("\n", error) });
                }
                return Json(new
                {
                    success = true,
                    redirect = Url.Action("Index", "Student", new { area = "Admin" })
                });
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
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Data", "MauSV.xlsx");

            // Kiểm tra nếu file tồn tại
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("File không tồn tại.");
            }

            // Lấy nội dung file
            var fileBytes = System.IO.File.ReadAllBytes(filePath);

            // Trả file về client
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "MauSV.xlsx");
        }

        public IActionResult DownloadListStudent(int? StudyYearId, int? DeptId, int? SpecializationId)
        {
            var query = _context.Students.AsQueryable();
            StudyYear? studyYear = null;
            Department? department = null;
            Major? major = null;
            if (StudyYearId.HasValue)
            {
                query = query.Where(ex => ex.Curriculum.StudyYearId == StudyYearId.Value);
                studyYear = _context.StudyYears.AsNoTracking().SingleOrDefault(x => x.Id == StudyYearId.Value);
            }

            if (DeptId.HasValue)
            {
                query = query.Where(ex => ex.DeptId == DeptId.Value);
                department = _context.Departments.AsNoTracking().SingleOrDefault(x => x.Id == DeptId.Value);
            }

            if (SpecializationId.HasValue)
            {
                query = query.Where(ex => ex.MajorId == SpecializationId.Value);
                major = _context.Majors.AsNoTracking().SingleOrDefault(x => x.Id == SpecializationId.Value);
            }

            var list = query.ToList();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Danh sách sinh viên");

                // Title row
                worksheet.Cells["A1:R1"].Merge = true;
                worksheet.Cells["A1"].Value = "DANH SÁCH SINH VIÊN";
                worksheet.Cells["A1"].Style.Font.Bold = true;
                worksheet.Cells["A1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                int row = 2;

                // Filters
                if (StudyYearId.HasValue)
                {
                    worksheet.Cells[$"A{row}"].Value = "Khóa";
                    worksheet.Cells[$"B{row}"].Value = $"{studyYear?.Number} | {studyYear?.StartYear?.StartYear} - {studyYear?.EndYear?.EndYear}";
                    row++;
                }

                if (DeptId.HasValue)
                {
                    worksheet.Cells[$"A{row}"].Value = "Khoa";
                    worksheet.Cells[$"B{row}"].Value = department?.Name;
                    row++;
                }

                if (SpecializationId.HasValue)
                {
                    worksheet.Cells[$"A{row}"].Value = "Ngành";
                    worksheet.Cells[$"B{row}"].Value = major?.Name;
                    row++;
                }

                // Table headers
                worksheet.Cells[$"A{row}"].Value = "STT";
                worksheet.Cells[$"B{row}"].Value = "Khóa";
                worksheet.Cells[$"C{row}"].Value = "Mã số sinh viên";
                worksheet.Cells[$"D{row}"].Value = "Họ lót";
                worksheet.Cells[$"E{row}"].Value = "Tên";
                worksheet.Cells[$"F{row}"].Value = "Ngày sinh";
                worksheet.Cells[$"G{row}"].Value = "Email";
                worksheet.Cells[$"H{row}"].Value = "SĐT";
                worksheet.Cells[$"I{row}"].Value = "Giới tính";
                worksheet.Cells[$"J{row}"].Value = "Mã lớp sinh viên";
                worksheet.Cells[$"K{row}"].Value = "Mã CTDT";
                worksheet.Cells[$"L{row}"].Value = "Mã khoa";
                worksheet.Cells[$"M{row}"].Value = "Mã ngành";
                worksheet.Cells[$"N{row}"].Value = "CCCD";
                worksheet.Cells[$"O{row}"].Value = "Nơi sinh";
                worksheet.Cells[$"P{row}"].Value = "Địa chỉ";
                worksheet.Cells[$"Q{row}"].Value = "Dân tộc";
                worksheet.Cells[$"R{row}"].Value = "Tôn giáo";
                worksheet.Row(row).Style.Font.Bold = true;

                row++;

                // Add student data
                int index = 1;
                foreach (var student in list)
                {
                    string[] nameParts = student.FullName.Trim().Split(' ');
                    string ho = string.Join(" ", nameParts, 0, nameParts.Length - 1);
                    string ten = nameParts[nameParts.Length - 1];
                    worksheet.Cells[$"A{row}"].Value = index++;
                    worksheet.Cells[$"B{row}"].Value = student.Curriculum.StudyYear.Number;
                    worksheet.Cells[$"C{row}"].Value = student.Id;
                    worksheet.Cells[$"D{row}"].Value = ho;
                    worksheet.Cells[$"E{row}"].Value = ten;
                    worksheet.Cells[$"F{row}"].Value = student.DayOfBirth.ToString("yyyy-MM-dd");
                    worksheet.Cells[$"G{row}"].Value = student.Email;
                    worksheet.Cells[$"H{row}"].Value = student.PhoneNo;
                    worksheet.Cells[$"I{row}"].Value = student.Sex;
                    worksheet.Cells[$"J{row}"].Value = student.StudentClass?.Code;
                    worksheet.Cells[$"K{row}"].Value = student.Curriculum.Code;
                    worksheet.Cells[$"L{row}"].Value = student.Dept.Code;
                    worksheet.Cells[$"M{row}"].Value = student.Major.Code;
                    worksheet.Cells[$"N{row}"].Value = student.NationId;
                    worksheet.Cells[$"O{row}"].Value = student.BirthPlace;
                    worksheet.Cells[$"P{row}"].Value = student.StreetAddress;
                    worksheet.Cells[$"Q{row}"].Value = student.Nation;
                    worksheet.Cells[$"R{row}"].Value = student.Religion;

                    row++;
                }

                // Adjust columns width
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Return Excel file
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                var fileName = "DanhSachSinhVien.xlsx";
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return File(stream, contentType, fileName);
            }
        }

    }
}
