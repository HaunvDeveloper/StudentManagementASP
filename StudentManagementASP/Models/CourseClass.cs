using System;
using System.Collections.Generic;

namespace StudentManagementASP.Models;

public partial class CourseClass
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public int SemesterId { get; set; }

    public int StartLesson { get; set; }

    public int EndLesson { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public int MaxQuantity { get; set; }

    public int CurrentQuantity { get; set; }

    public int CourseId { get; set; }

    public int? LecturerId { get; set; }

    public int? DefaultRoomId { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual Room? DefaultRoom { get; set; }

    public virtual LessonInfo EndLessonNavigation { get; set; } = null!;

    public virtual Lecturer? Lecturer { get; set; }

    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();

    public virtual Semester Semester { get; set; } = null!;

    public virtual LessonInfo StartLessonNavigation { get; set; } = null!;

    public virtual ICollection<StudentJoinClass> StudentJoinClasses { get; set; } = new List<StudentJoinClass>();
}
