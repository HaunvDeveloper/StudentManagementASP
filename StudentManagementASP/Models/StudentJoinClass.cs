using System;
using System.Collections.Generic;

namespace StudentManagementASP.Models;

public partial class StudentJoinClass
{
    public int Id { get; set; }

    public string StudentId { get; set; } = null!;

    public int CourseClassId { get; set; }

    public DateTime DateJoin { get; set; }

    public virtual CourseClass CourseClass { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
