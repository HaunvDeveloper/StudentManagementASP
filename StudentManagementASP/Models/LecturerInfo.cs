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

    public string? ProvinceAddress { get; set; }

    public string? DistrictAddress { get; set; }

    public string? WardAddress { get; set; }

    public string? StreetAddress { get; set; }

    public string Sex { get; set; } = null!;

    public string? PhoneNo { get; set; }

    public string? Nation { get; set; }

    public string? Religion { get; set; }

    public virtual Lecturer Lecturer { get; set; } = null!;
}
