using System;
using System.Collections.Generic;

namespace StudentManagementASP.Models;

public partial class Semester
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public int SchoolYearDetailId { get; set; }

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

    public virtual StudyYearDetail SchoolYearDetail { get; set; } = null!;
}
