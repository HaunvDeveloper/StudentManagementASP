using System;
using System.Collections.Generic;

namespace StudentManagementASP.Models;

public partial class Ward
{
    public int Code { get; set; }

    public string? Name { get; set; }

    public int DistrictCode { get; set; }

    public virtual District DistrictCodeNavigation { get; set; } = null!;

    public virtual ICollection<Lecturer> Lecturers { get; set; } = new List<Lecturer>();

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
