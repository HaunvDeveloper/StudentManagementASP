namespace StudentManagementASP.ViewModels
{
    public class LecturerViewModel
    {
        public int Id { get; set; }

        public string FullName { get; set; } = null!;

        public string? Email { get; set; }

        public DateTime DayOfBirth { get; set; }

        public DateTime HiredDate { get; set; }

        public int DeptId { get; set; }

        public int? UserId { get; set; }

        public string BirthName { get; set; } = null!;

        public string NationId { get; set; } = null!;

        public string BirthPlace { get; set; } = null!;

        public int? ProvinceCode { get; set; }

        public int? DistrictCode { get; set; }

        public int? WardCode { get; set; }

        public string? StreetAddress { get; set; }

        public string Sex { get; set; } = null!;

        public string? PhoneNo { get; set; }

        public string? Nation { get; set; }

        public string? Religion { get; set; }
    }
}
