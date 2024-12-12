using System;
using System.Collections.Generic;

namespace StudentManagementASP.Models;

public partial class LecturerInfo
{
    public int Id { get; set; }

    public int LecturerId { get; set; }

    public string BirthName { get; set; } = null!;

    public string NationId { get; set; } = null!;

    public string BirthPlace { get; set; } = null!;

    public string? TempAddress { get; set; }

    public string PermanentAddress { get; set; } = null!;

    public string Sex { get; set; } = null!;

    public virtual Lecturer Lecturer { get; set; } = null!;
}
