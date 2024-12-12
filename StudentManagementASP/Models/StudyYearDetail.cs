using System;
using System.Collections.Generic;

namespace StudentManagementASP.Models;

public partial class StudyYearDetail
{
    public int Id { get; set; }

    public DateTime StartYear { get; set; }

    public DateTime EndYear { get; set; }

    public int StudyYearId { get; set; }

    public virtual ICollection<Semester> Semesters { get; set; } = new List<Semester>();

    public virtual StudyYear StudyYear { get; set; } = null!;
}
