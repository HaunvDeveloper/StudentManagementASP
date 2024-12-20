using StudentManagementASP.Models;

namespace StudentManagementASP.ViewModels
{
    public class SubjectListViewModel
    {
        public List<Subject> Subjects { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
