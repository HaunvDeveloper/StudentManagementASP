using System;
using System.Collections.Generic;

namespace StudentManagementASP.Models;

public partial class District
{
    public int Code { get; set; }

    public string? Name { get; set; }

    public int ProvinceCode { get; set; }

    public virtual ICollection<LecturerInfo> LecturerInfos { get; set; } = new List<LecturerInfo>();

    public virtual Province ProvinceCodeNavigation { get; set; } = null!;

    public virtual ICollection<StudentInfo> StudentInfos { get; set; } = new List<StudentInfo>();

    public virtual ICollection<Ward> Wards { get; set; } = new List<Ward>();
}
