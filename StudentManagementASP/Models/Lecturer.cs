using System;
using System.Collections.Generic;

namespace StudentManagementASP.Models;

public partial class Lecturer
{
    public int Id { get; set; }

    public string FullName { get; set; } = null!;

    public string? Email { get; set; }

    public DateTime DayOfBirth { get; set; }

    public DateTime HiredDate { get; set; }

    public int DeptId { get; set; }

    public int? UserId { get; set; }

    public virtual ICollection<CourseClass> CourseClasses { get; set; } = new List<CourseClass>();

    public virtual Department Dept { get; set; } = null!;

    public virtual ICollection<LecturerInfo> LecturerInfos { get; set; } = new List<LecturerInfo>();

    public virtual ICollection<StudentClass> StudentClasses { get; set; } = new List<StudentClass>();

    public virtual User? User { get; set; }
}
