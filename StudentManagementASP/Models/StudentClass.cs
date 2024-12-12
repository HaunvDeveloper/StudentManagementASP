using System;
using System.Collections.Generic;

namespace StudentManagementASP.Models;

public partial class StudentClass
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public int? LecturerId { get; set; }

    public virtual Lecturer? Lecturer { get; set; }

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
