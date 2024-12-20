using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OfficeOpenXml;
using StudentManagementASP.Models;
using StudentManagementASP.ViewModels;

namespace StudentManagementASP.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin,manager")]
    public class LecturerController : Controller
    {

        private readonly StudentManagementContext _context;
        private readonly IWebHostEnvironment _environment;

        public LecturerController(StudentManagementContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
            ExcelPackage.LicenseContext = LicenseContext.Commercial;
        }


        public IActionResult Index(int? deptId, int page = 1)
        {
            int pageSize = 10;

            var query = _context.Lecturers.AsNoTracking();

            if (deptId.HasValue)
            {
                query = query.Where(x => x.DeptId == deptId.Value);
            }

            var totalLecturers = query.Count();

            var lecturers = query
                .Skip((page - 1) * pageSize)  // Skip previous pages
                .Take(pageSize)              // Take the current page size
                .ToList();

            var model = new LecturerListViewModel
            {
                Lecturers = lecturers,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalLecturers / pageSize)
            };

            // Load department list for the dropdown
            ViewBag.Depts = new SelectList(_context.Departments.AsNoTracking().ToList(), "Id", "Name");

            return View(model);
        }


        public IActionResult Create()
        {
            ViewBag.Depts = new SelectList(_context.Departments.AsNoTracking().ToList(), "Id", "Name");
            ViewBag.NewId = _context.Lecturers.AsNoTracking().OrderByDescending(x => x.Id).First().Id + 1;
            string filePath = Path.Combine(_environment.WebRootPath, "Data", "nation.json");
            var jsonData = System.IO.File.ReadAllText(filePath);
            var ethnicList = JsonConvert.DeserializeObject<List<NationViewModel>>(jsonData);
            ViewBag.NationNames = new SelectList(ethnicList, "EthnicName", "EthnicName");

            filePath = Path.Combine(_environment.WebRootPath, "Data", "religion.json");
            jsonData = System.IO.File.ReadAllText(filePath);
            var religionList = JsonConvert.DeserializeObject<List<ReligionViewModel>>(jsonData);
            ViewBag.ReligionNames = new SelectList(religionList, "ReligionName", "ReligionName");
            return View();
        }
        [HttpPost]
        public IActionResult Create(Models.Lecturer model, string Password)
        {
            try
            {
                model.HiredDate = DateTime.Now;
                model.User = new User
                {
                    Username = model.Id.ToString(),
                    Password = string.IsNullOrEmpty(Password) ? model.Id.ToString() : Password,
                    FullName = model.FullName,
                    Email = model.Email,
                    IsBlock = false,
                    AuthId = 3,
                    DayOfBirth = model.DayOfBirth,
                };
                _context.Lecturers.Add(model);
                _context.SaveChanges();
                return Json(new { success = true, redirect = Url.Action("Index", "Lecturer", new { area = "Admin" }) });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.ToString()});

            }
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
                var listLecturer = new List<Models.Lecturer>();
                var depts = _context.Departments.AsNoTracking().ToList();
                List<string> error = new List<string>();
                int succNum = 0;
                for (int row = 6; row <= rowCount; row++)
                {
                    try
                    {
                        int Id = 0;
                        if (!int.TryParse(worksheet.Cells[row, 2].Text.Trim(), out Id))
                        {
                            break;
                        }
                        var deptId = depts.FirstOrDefault(x => x.Code == worksheet.Cells[row, 9].Text.Trim())?.Id;

                        if (deptId == null)
                        {
                            error.Add(worksheet.Cells[row, 2].Text.Trim() + " không có mã khoa hoặc mã khoa không tồn tại!");
                            continue;
                        }

                        var lecturer = new Models.Lecturer
                        {
                            Id = Id,
                            FullName = worksheet.Cells[row, 3].Text.Trim() + " " + worksheet.Cells[row, 4].Text.Trim(),
                            DayOfBirth = DateTime.Parse(worksheet.Cells[row, 5].Text.Trim()),
                            Email = worksheet.Cells[row, 6].Text.Trim(),
                            DeptId = deptId ?? 0,
                            HiredDate = DateTime.Now,
                            PhoneNo = worksheet.Cells[row, 7].Text.Trim(),
                            Sex = worksheet.Cells[row, 8].Text.Trim(),
                            NationId = worksheet.Cells[row, 10].Text.Trim(),
                            BirthPlace = worksheet.Cells[row, 11].Text.Trim(),
                            StreetAddress = worksheet.Cells[row, 12].Text.Trim(),
                            Nation = worksheet.Cells[row, 13].Text.Trim(),
                            Religion = worksheet.Cells[row, 14].Text.Trim(),
                            User = new User()
                            {
                                Username = Id.ToString(),
                                Password = Id.ToString(),
                                FullName = worksheet.Cells[row, 3].Text.Trim() + " " + worksheet.Cells[row, 4].Text.Trim(),
                                Email = worksheet.Cells[row, 6].Text.Trim(),
                                DayOfBirth = DateTime.Parse(worksheet.Cells[row, 5].Text.Trim()),
                                IsBlock = false,
                                AuthId = 3
                            }
                        };
                        listLecturer.Add(lecturer);
                        succNum++;
                    }
                    catch (Exception ex)
                    {
                        error.Add(worksheet.Cells[row, 3].Text.Trim() + " has error: " + ex.ToString());
                        continue;
                    }
                }

                _context.Lecturers.AddRange(listLecturer);
                await _context.SaveChangesAsync();
                if (succNum > 0)
                {
                    return Json(new { success = true, redirect = Url.Action("Index", "Lecturer", new { area = "Admin" }), message = "Has some error:" + string.Join("\n", error) });
                }
                return Json(new
                {
                    success = true,
                    redirect = Url.Action("Index", "Lecturer", new { area = "Admin" })
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
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Data", "MauGV.xlsx");

            // Kiểm tra nếu file tồn tại
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("File không tồn tại.");
            }

            // Lấy nội dung file
            var fileBytes = System.IO.File.ReadAllBytes(filePath);

            // Trả file về client
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "MauGV.xlsx");
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var model = _context.Lecturers.Find(id);
            if (model == null)
            {
                return NotFound();
            }
            ViewBag.Depts = new SelectList(_context.Departments.AsNoTracking().ToList(), "Id", "Name");
            string filePath = Path.Combine(_environment.WebRootPath, "Data", "nation.json");
            var jsonData = System.IO.File.ReadAllText(filePath);
            var ethnicList = JsonConvert.DeserializeObject<List<NationViewModel>>(jsonData);
            ViewBag.NationNames = new SelectList(ethnicList, "EthnicName", "EthnicName");

            filePath = Path.Combine(_environment.WebRootPath, "Data", "religion.json");
            jsonData = System.IO.File.ReadAllText(filePath);
            var religionList = JsonConvert.DeserializeObject<List<ReligionViewModel>>(jsonData);
            ViewBag.ReligionNames = new SelectList(religionList, "ReligionName", "ReligionName");
            return View(model);
        }

        [HttpPost]
        public IActionResult Edit(Models.Lecturer model)
        {
            try
            {
                var oldModel = _context.Lecturers
                    .Include(x => x.User)
                    .SingleOrDefault(x => x.Id == model.Id);

                if(oldModel != null)
                {
                    oldModel.Id = model.Id;
                    oldModel.FullName = model.FullName;
                    oldModel.DeptId = model.DeptId;
                    oldModel.DayOfBirth = model.DayOfBirth;
                    oldModel.HiredDate = DateTime.Now;
                    oldModel.Email = model.Email;
                    oldModel.User.FullName = model.FullName;
                    oldModel.User.Email = model.Email;
                    oldModel.User.DayOfBirth = model.DayOfBirth;
                    oldModel.ProvinceCode = model.ProvinceCode;
                    oldModel.WardCode = model.WardCode;
                    oldModel.DistrictCode = model.DistrictCode;
                    oldModel.BirthPlace = model.BirthPlace;
                    oldModel.NationId = model.NationId;
                    oldModel.Nation = model.Nation;
                    oldModel.Religion = model.Religion;
                    oldModel.Sex = model.Sex;
                    oldModel.StreetAddress = model.StreetAddress;
                    oldModel.PhoneNo = model.PhoneNo;
                    _context.SaveChanges();
                }

                return Json(new { success = true, redirect= Url.Action("Index") });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        public IActionResult DownloadListLecturer(int? DeptId)
        {
            var query = _context.Lecturers.AsQueryable();
            Department? department = null;
            if (DeptId.HasValue)
            {
                query = query.Where(ex => ex.DeptId == DeptId.Value);
                department = _context.Departments.AsNoTracking().SingleOrDefault(x => x.Id == DeptId.Value);
            }

            var list = query.ToList();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Danh sách giảng viên");

                // Title row
                worksheet.Cells["A1:N1"].Merge = true;
                worksheet.Cells["A1"].Value = "DANH SÁCH GIẢNG VIÊN";
                worksheet.Cells["A1"].Style.Font.Bold = true;
                worksheet.Cells["A1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                int row = 2;

                if (DeptId.HasValue)
                {
                    worksheet.Cells[$"A{row}"].Value = "Khoa";
                    worksheet.Cells[$"B{row}"].Value = department?.Name;
                    row++;
                }


                // Table headers
                worksheet.Cells[$"A{row}"].Value = "STT";
                worksheet.Cells[$"B{row}"].Value = "Mã số giảng viên";
                worksheet.Cells[$"C{row}"].Value = "Họ lót";
                worksheet.Cells[$"D{row}"].Value = "Tên";
                worksheet.Cells[$"E{row}"].Value = "Ngày sinh";
                worksheet.Cells[$"F{row}"].Value = "Email";
                worksheet.Cells[$"G{row}"].Value = "SĐT";
                worksheet.Cells[$"H{row}"].Value = "Giới tính";
                worksheet.Cells[$"I{row}"].Value = "Mã khoa";
                worksheet.Cells[$"J{row}"].Value = "CCCD";
                worksheet.Cells[$"K{row}"].Value = "Nơi sinh";
                worksheet.Cells[$"L{row}"].Value = "Địa chỉ";
                worksheet.Cells[$"M{row}"].Value = "Dân tộc";
                worksheet.Cells[$"N{row}"].Value = "Tôn giáo";

                worksheet.Row(row).Style.Font.Bold = true;

                row++;

                // Add student data
                int index = 1;
                foreach (var lecturer in list)
                {
                    string[] nameParts = lecturer.FullName.Trim().Split(' ');
                    string ho = string.Join(" ", nameParts, 0, nameParts.Length - 1);
                    string ten = nameParts[nameParts.Length - 1];
                    worksheet.Cells[$"A{row}"].Value = index++;
                    worksheet.Cells[$"B{row}"].Value = lecturer.Id;
                    worksheet.Cells[$"C{row}"].Value = ho;
                    worksheet.Cells[$"D{row}"].Value = ten;
                    worksheet.Cells[$"E{row}"].Value = lecturer.DayOfBirth.ToString("yyyy-MM-dd");
                    worksheet.Cells[$"F{row}"].Value = lecturer.Email;
                    worksheet.Cells[$"G{row}"].Value = lecturer.PhoneNo;
                    worksheet.Cells[$"H{row}"].Value = lecturer.Sex;
                    worksheet.Cells[$"I{row}"].Value = lecturer.Dept.Code;
                    worksheet.Cells[$"J{row}"].Value = lecturer.NationId;
                    worksheet.Cells[$"K{row}"].Value = lecturer.BirthPlace;
                    worksheet.Cells[$"L{row}"].Value = $"{lecturer.StreetAddress}, {lecturer.WardCodeNavigation?.Name}, {lecturer.DistrictCodeNavigation?.Name}, {lecturer.ProvinceCodeNavigation?.Name}";
                    worksheet.Cells[$"M{row}"].Value = lecturer.Nation;
                    worksheet.Cells[$"N{row}"].Value = lecturer.Religion;

                    row++;
                }

                // Adjust columns width
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Return Excel file
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                var fileName = "DanhSachGiangVien.xlsx";
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return File(stream, contentType, fileName);
            }
        }
    }
}
