using StudentManagementASP.Models;

namespace StudentManagementASP.ViewModels
{
    public class MajorListViewModel
    {
        public List<Major> Majors { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
