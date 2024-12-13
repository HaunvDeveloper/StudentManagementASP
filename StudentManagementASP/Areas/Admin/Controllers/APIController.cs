using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagementASP.Models;

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

        

    }
}
