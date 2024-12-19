using System;
using System.Collections.Generic;

namespace StudentManagementASP.Models;

public partial class Province
{
    public int Code { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<District> Districts { get; set; } = new List<District>();

    public virtual ICollection<Lecturer> Lecturers { get; set; } = new List<Lecturer>();

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
