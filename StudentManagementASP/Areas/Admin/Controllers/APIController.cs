using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagementASP.Models;
using StudentManagementASP.ViewModels;

namespace StudentManagementASP.Areas.Admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class APIController : ControllerBase
    {
        private readonly StudentManagementContext _context;

        public APIController(StudentManagementContext context)
        {
            _context = context;
        }

        [HttpPost("GetSpecializationByDept")]
        public async Task<IActionResult> GetSpecializationByDept(int deptid)
        {
            // Lấy danh sách ngành học theo DeptId từ _context.Major
            var specializations = await _context.Majors
                .Where(m => m.DeptId == deptid)
                .Select(m => new
                {
                    m.Id,
                    m.Name
                })
                .ToListAsync();

            

            return Ok(specializations);
        }


        [HttpPost("GetCurriculumByYearId")]
        public async Task<IActionResult> GetCurriculumByYearId(int yearId)
        {
            // Lấy danh sách ngành học theo DeptId từ _context.Major
            var curriculumList = _context.Curricula.AsNoTracking()
                .Where(x => x.StudyYearId == yearId)
                .Select(x => new
                {
                    x.Id,x.Name,x.Code
                })
                .ToList();
            return Ok(curriculumList);
        }



        [HttpGet("GetSubject")]
        public IActionResult GetSubject(int id)
        {
            // Lấy danh sách ngành học theo DeptId từ _context.Major
            var subject = _context.Subjects.AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new
                {
                    x.Code,
                    x.Name,
                    x.Id,
                    x.DefaultCredits,
                    x.DefaultLesson,
                })
                .FirstOrDefault();
            if(subject == null)
            {
                return NotFound();
            }
            return Ok(subject);
        }

        [HttpGet("GetYearDetail")]
        public IActionResult GetYearDetail(int yearId)
        {
            var yearStudy = _context.StudyYears.AsNoTracking().FirstOrDefault(x => x.Id == yearId);
            if(yearStudy == null)
            {
                return NotFound();
            }
            var list = _context.StudyYearDetails.AsNoTracking().Where(x => x.StartYear >= yearStudy.StartYearId && x.EndYear <= yearStudy.EndYearId);
            
            return Ok(list);
        }

        [HttpGet("GetSemesterByYearDetail")]
        public IActionResult GetSemesterByYearDetail(int yearDetailId)
        {
            var semester = _context.Semesters.AsNoTracking()
                .Where(x => x.SchoolYearDetailId == yearDetailId)
                .Select(x => new SemesterViewModel()
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name,
                })
                .ToList();
            return Ok(semester);
        }

        [HttpGet("GetNewCodeCourseClass")]
        public IActionResult GetNewCodeCourseClass(int subjectId, int semesterId)
        {
            var subject = _context.Subjects.AsNoTracking().SingleOrDefault(x => x.Id == subjectId);
            if(subject == null)
            {
                return NotFound();
            }
            int count = _context.CourseClasses.AsNoTracking()
                .Where(x => x.SubjectId == subjectId && x.SemesterId == semesterId)
                .OrderByDescending(x => x.Code)
                .Count() + 1;
            
            return Ok(new { code = subject.Code + count.ToString("00"), name = subject.Name, lessonNo = subject.DefaultLesson });
        }
    }
}
