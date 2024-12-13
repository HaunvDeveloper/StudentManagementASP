using System;
using System.Collections.Generic;

namespace StudentManagementASP.Models;

public partial class LessonInfo
{
    public int Id { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public virtual ICollection<CourseClass> CourseClassEndLessonNavigations { get; set; } = new List<CourseClass>();

    public virtual ICollection<CourseClass> CourseClassStartLessonNavigations { get; set; } = new List<CourseClass>();

    public virtual ICollection<Lesson> LessonEndLessonNavigations { get; set; } = new List<Lesson>();

    public virtual ICollection<Lesson> LessonStartLessonNavigations { get; set; } = new List<Lesson>();
}
