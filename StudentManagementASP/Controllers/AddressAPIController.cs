using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagementASP.Models;
using StudentManagementASP.Services;
using StudentManagementASP.ViewModels;

namespace StudentManagementASP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AddressAPIController : ControllerBase
    {
        private readonly StudentManagementContext _context;

        public AddressAPIController(StudentManagementContext context)
        {
            _context = context;
        }

        [HttpGet("GetProvince")]
        public IActionResult GetProvince()
        {
            var list = _context.Provinces.AsNoTracking()
                .Select(x => new ProvinceViewModel()
                {
                    Code = x.Code,
                    Name = x.Name,
                })
                .ToList();
            return Ok(list);
        }

        [HttpGet("GetDistrictByProvince")]
        public IActionResult GetDistrictByProvince(int provinceCode)
        {
            var list = _context.Districts.AsNoTracking().Where(x => x.ProvinceCode == provinceCode)
                .Select(x => new DistrictViewModel()
                {
                    Code = x.Code,
                    Name = x.Name,
                    ProvinceCode = x.ProvinceCode
                })
                .ToList();
            return Ok(list);
        }

        [HttpGet("GetWardByDistrict")]
        public IActionResult GetWardByDistrict(int districtCode)
        {
            var list = _context.Wards.AsNoTracking().Where(x => x.DistrictCode == districtCode)
                .Select(x => new WardViewModel()
                {
                    Code = x.Code,
                    Name = x.Name,
                    DistrictCode = x.DistrictCode
                })
                .ToList();
            return Ok(list);
        }
    }
}
