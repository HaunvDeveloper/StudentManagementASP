using StudentManagementASP.Models;

namespace StudentManagementASP.ViewModels
{
    public class StudentListViewModel
    {
        public List<Student> Students { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }

}
