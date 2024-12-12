using System;
using System.Collections.Generic;

namespace StudentManagementASP.Models;

public partial class StudyYear
{
    public int Id { get; set; }

    public int Number { get; set; }

    public DateTime StartYear { get; set; }

    public DateTime EndYear { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime ExpireDate { get; set; }

    public virtual ICollection<Curriculum> Curricula { get; set; } = new List<Curriculum>();

    public virtual ICollection<StudyYearDetail> StudyYearDetails { get; set; } = new List<StudyYearDetail>();
}
