using System;
using System.Collections.Generic;

namespace StudentManagementASP.Models;

public partial class Province
{
    public int Code { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<District> Districts { get; set; } = new List<District>();

    public virtual ICollection<LecturerInfo> LecturerInfos { get; set; } = new List<LecturerInfo>();

    public virtual ICollection<StudentInfo> StudentInfos { get; set; } = new List<StudentInfo>();
}
