using System;
using System.Collections.Generic;

namespace StudentManagementASP.Models;

public partial class Ward
{
    public int Code { get; set; }

    public string? Name { get; set; }

    public int DistrictCode { get; set; }

    public virtual District DistrictCodeNavigation { get; set; } = null!;

    public virtual ICollection<LecturerInfo> LecturerInfos { get; set; } = new List<LecturerInfo>();

    public virtual ICollection<StudentInfo> StudentInfos { get; set; } = new List<StudentInfo>();
}
