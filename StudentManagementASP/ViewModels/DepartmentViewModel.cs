using StudentManagementASP.Models;

namespace StudentManagementASP.ViewModels
{
    public class DepartmentViewModel
    {
        public List<Department> Departments { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
