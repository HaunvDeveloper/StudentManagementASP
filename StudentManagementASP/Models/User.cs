using System;
using System.Collections.Generic;

namespace StudentManagementASP.Models;

public partial class User
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? FullName { get; set; }

    public string? Email { get; set; }

    public DateTime? DayOfBirth { get; set; }

    public string? Otp { get; set; }

    public DateTime? OtplastestSend { get; set; }

    public bool IsBlock { get; set; }

    public int AuthId { get; set; }

    public virtual Authentication Auth { get; set; } = null!;

    public virtual ICollection<Lecturer> Lecturers { get; set; } = new List<Lecturer>();

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
