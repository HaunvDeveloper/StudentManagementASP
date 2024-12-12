using System;
using System.Collections.Generic;

namespace StudentManagementASP.Models;

public partial class Course
{
    public int Id { get; set; }

    public int Lesson { get; set; }

    public int Credits { get; set; }

    public int CurriculumId { get; set; }

    public int SemesterId { get; set; }

    public int TypeId { get; set; }

    public int SubjectId { get; set; }

    public string? Infomation { get; set; }

    public virtual ICollection<CourseClass> CourseClasses { get; set; } = new List<CourseClass>();

    public virtual Curriculum Curriculum { get; set; } = null!;

    public virtual Semester Semester { get; set; } = null!;

    public virtual Subject Subject { get; set; } = null!;

    public virtual CourseType Type { get; set; } = null!;
}
