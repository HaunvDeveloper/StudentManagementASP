using System;
using System.Collections.Generic;

namespace StudentManagementASP.Models;

public partial class StudyYear
{
    public int Id { get; set; }

    public int Number { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime ExpireDate { get; set; }

    public int? StartYearId { get; set; }

    public int? EndYearId { get; set; }

    public virtual ICollection<Curriculum> Curricula { get; set; } = new List<Curriculum>();

    public virtual StudyYearDetail? EndYear { get; set; }

    public virtual StudyYearDetail? StartYear { get; set; }
}
