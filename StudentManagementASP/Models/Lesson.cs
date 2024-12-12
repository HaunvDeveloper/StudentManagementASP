using System;
using System.Collections.Generic;

namespace StudentManagementASP.Models;

public partial class Lesson
{
    public int Id { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public int CourseClassId { get; set; }

    public int? RoomId { get; set; }

    public virtual CourseClass CourseClass { get; set; } = null!;

    public virtual Room? Room { get; set; }

    public virtual ICollection<StudentJoinLesson> StudentJoinLessons { get; set; } = new List<StudentJoinLesson>();
}
