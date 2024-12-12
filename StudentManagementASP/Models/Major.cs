using System;
using System.Collections.Generic;

namespace StudentManagementASP.Models;

public partial class Major
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public int? DeptId { get; set; }

    public virtual ICollection<Curriculum> Curricula { get; set; } = new List<Curriculum>();

    public virtual Department? Dept { get; set; }

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
