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

    public string NationId { get; set; } = null!;

    public string BirthPlace { get; set; } = null!;

    public int? ProvinceCode { get; set; }

    public int? DistrictCode { get; set; }

    public int? WardCode { get; set; }

    public string? StreetAddress { get; set; }

    public string Sex { get; set; } = null!;

    public string? FaceData { get; set; }

    public string? PhoneNo { get; set; }

    public string? Nation { get; set; }

    public string? Religion { get; set; }

    public virtual Curriculum Curriculum { get; set; } = null!;

    public virtual Department Dept { get; set; } = null!;

    public virtual District? DistrictCodeNavigation { get; set; }

    public virtual Major Major { get; set; } = null!;

    public virtual Province? ProvinceCodeNavigation { get; set; }

    public virtual StudentClass? StudentClass { get; set; }

    public virtual ICollection<StudentJoinClass> StudentJoinClasses { get; set; } = new List<StudentJoinClass>();

    public virtual ICollection<StudentJoinLesson> StudentJoinLessons { get; set; } = new List<StudentJoinLesson>();

    public virtual User? User { get; set; }

    public virtual Ward? WardCodeNavigation { get; set; }
}
