using System;
using System.Collections.Generic;

namespace StudentManagementASP.Models;

public partial class Student
{
    public string Id { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public DateTime DayOfBirth { get; set; }

    public string? Email { get; set; }

    public string Status { get; set; } = null!;

    public int? StudentClassId { get; set; }

    public int CurriculumId { get; set; }

    public int DeptId { get; set; }

    public int MajorId { get; set; }

    public int? UserId { get; set; }

    public virtual Curriculum Curriculum { get; set; } = null!;

    public virtual Department Dept { get; set; } = null!;

    public virtual Major Major { get; set; } = null!;

    public virtual StudentClass? StudentClass { get; set; }

    public virtual ICollection<StudentInfo> StudentInfos { get; set; } = new List<StudentInfo>();

    public virtual ICollection<StudentJoinClass> StudentJoinClasses { get; set; } = new List<StudentJoinClass>();

    public virtual ICollection<StudentJoinLesson> StudentJoinLessons { get; set; } = new List<StudentJoinLesson>();

    public virtual User? User { get; set; }
}
