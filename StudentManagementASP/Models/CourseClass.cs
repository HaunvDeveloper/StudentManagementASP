using System;
using System.Collections.Generic;

namespace StudentManagementASP.Models;

public partial class CourseClass
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public int SemesterId { get; set; }

    public string? WeakDays { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public int MaxQuantity { get; set; }

    public int CurrentQuantity { get; set; }

    public int? CourseId { get; set; }

    public int? LecturerId { get; set; }

    public int? SubjectId { get; set; }

    public int? StudentClassId { get; set; }

    public int? DefaultRoomId { get; set; }

    public virtual Course? Course { get; set; }

    public virtual Room? DefaultRoom { get; set; }

    public virtual ICollection<Device> Devices { get; set; } = new List<Device>();

    public virtual Lecturer? Lecturer { get; set; }

    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();

    public virtual Semester Semester { get; set; } = null!;

    public virtual StudentClass? StudentClass { get; set; }

    public virtual ICollection<StudentJoinClass> StudentJoinClasses { get; set; } = new List<StudentJoinClass>();

    public virtual Subject? Subject { get; set; }
}
