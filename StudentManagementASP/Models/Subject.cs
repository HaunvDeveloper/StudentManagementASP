using System;
using System.Collections.Generic;

namespace StudentManagementASP.Models;

public partial class Subject
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public int DefaultCredits { get; set; }

    public int DeptId { get; set; }

    public int? DefaultLesson { get; set; }

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
}
