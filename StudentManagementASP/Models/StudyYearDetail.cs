using System;
using System.Collections.Generic;

namespace StudentManagementASP.Models;

public partial class StudyYearDetail
{
    public int Id { get; set; }

    public int StartYear { get; set; }

    public int EndYear { get; set; }

    public virtual ICollection<Semester> Semesters { get; set; } = new List<Semester>();

    public virtual ICollection<StudyYear> StudyYearEndYears { get; set; } = new List<StudyYear>();

    public virtual ICollection<StudyYear> StudyYearStartYears { get; set; } = new List<StudyYear>();
}
