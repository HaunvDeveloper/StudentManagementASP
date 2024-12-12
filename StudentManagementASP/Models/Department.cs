using System;
using System.Collections.Generic;

namespace StudentManagementASP.Models;

public partial class Department
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public DateTime DateFound { get; set; }

    public virtual ICollection<Lecturer> Lecturers { get; set; } = new List<Lecturer>();

    public virtual ICollection<Major> Majors { get; set; } = new List<Major>();

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
