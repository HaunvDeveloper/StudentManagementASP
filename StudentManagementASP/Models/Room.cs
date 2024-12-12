using System;
using System.Collections.Generic;

namespace StudentManagementASP.Models;

public partial class Room
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Settlement { get; set; } = null!;

    public string Address { get; set; } = null!;

    public virtual ICollection<CourseClass> CourseClasses { get; set; } = new List<CourseClass>();

    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}
