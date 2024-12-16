using System;
using System.Collections.Generic;

namespace StudentManagementASP.Models;

public partial class LecturerInfo
{
    public int Id { get; set; }

    public int LecturerId { get; set; }

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

    public virtual District? DistrictCodeNavigation { get; set; }

    public virtual Lecturer Lecturer { get; set; } = null!;

    public virtual Province? ProvinceCodeNavigation { get; set; }

    public virtual Ward? WardCodeNavigation { get; set; }
}
