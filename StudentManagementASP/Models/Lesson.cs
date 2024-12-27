using System;
using System.Collections.Generic;

namespace StudentManagementASP.Models;

public partial class Lesson
{
    public int Id { get; set; }

    public int StartLesson { get; set; }

    public int EndLesson { get; set; }

    public DateOnly? Date { get; set; }

    public int CourseClassId { get; set; }

    public int? RoomId { get; set; }

    public virtual CourseClass CourseClass { get; set; } = null!;

    public virtual ICollection<Device> Devices { get; set; } = new List<Device>();

    public virtual LessonInfo EndLessonNavigation { get; set; } = null!;

    public virtual Room? Room { get; set; }

    public virtual LessonInfo StartLessonNavigation { get; set; } = null!;

    public virtual ICollection<StudentJoinLesson> StudentJoinLessons { get; set; } = new List<StudentJoinLesson>();
}
