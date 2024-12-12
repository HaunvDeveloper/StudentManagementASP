using System;
using System.Collections.Generic;

namespace StudentManagementASP.Models;

public partial class Curriculum
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public int StudyYearId { get; set; }

    public int MajorId { get; set; }

    public int? TotalCredits { get; set; }

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

    public virtual Major Major { get; set; } = null!;

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();

    public virtual StudyYear StudyYear { get; set; } = null!;
}
