using System;
using System.Collections.Generic;

namespace StudentManagementASP.Models;

public partial class StudentJoinLesson
{
    public int Id { get; set; }

    public string StudentId { get; set; } = null!;

    public int LessonId { get; set; }

    public DateTime JoinTime { get; set; }

    public string Status { get; set; } = null!;

    public int? LateLessons { get; set; }

    public virtual Lesson Lesson { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
