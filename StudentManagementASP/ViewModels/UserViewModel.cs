using StudentManagementASP.Models;

namespace StudentManagementASP.ViewModels
{
    public class UserViewModel
    {
        public UserViewModel() { }

        public UserViewModel(User user)
        {
            Id = user.Id;
            Username = user.Username;
            Email = user.Email;
            Password = user.Password;
            DayOfBirth = user.DayOfBirth;
            Otp = user.Otp;
            OtplastestSend = user.OtplastestSend;
            IsBlock = user.IsBlock;
            AuthId = user.AuthId;
        }

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
    }
}
