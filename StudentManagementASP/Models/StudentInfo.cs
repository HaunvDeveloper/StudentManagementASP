using System;
using System.Collections.Generic;

namespace StudentManagementASP.Models;

public partial class StudentInfo
{
    public int Id { get; set; }

    public string StudentId { get; set; } = null!;

    public string BirthName { get; set; } = null!;

    public string NationId { get; set; } = null!;

    public string BirthPlace { get; set; } = null!;

    public string? TempAddress { get; set; }

    public string PermanentAddress { get; set; } = null!;

    public string Sex { get; set; } = null!;

    public string? FaceData { get; set; }

    public string? PhoneNo { get; set; }

    public virtual Student Student { get; set; } = null!;
}
