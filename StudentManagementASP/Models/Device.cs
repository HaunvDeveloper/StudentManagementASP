using System;
using System.Collections.Generic;

namespace StudentManagementASP.Models;

public partial class Device
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string? Name { get; set; }

    public int? RoomId { get; set; }

    public int? CourseClassId { get; set; }

    public int? LessonId { get; set; }

    public int? UserId { get; set; }

    public bool IsActive { get; set; }

    public virtual CourseClass? CourseClass { get; set; }

    public virtual Lesson? Lesson { get; set; }

    public virtual Room? Room { get; set; }

    public virtual User? User { get; set; }
}
