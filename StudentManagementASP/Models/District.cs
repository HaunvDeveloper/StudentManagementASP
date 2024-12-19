using System;
using System.Collections.Generic;

namespace StudentManagementASP.Models;

public partial class District
{
    public int Code { get; set; }

    public string? Name { get; set; }

    public int ProvinceCode { get; set; }

    public virtual ICollection<Lecturer> Lecturers { get; set; } = new List<Lecturer>();

    public virtual Province ProvinceCodeNavigation { get; set; } = null!;

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();

    public virtual ICollection<Ward> Wards { get; set; } = new List<Ward>();
}
