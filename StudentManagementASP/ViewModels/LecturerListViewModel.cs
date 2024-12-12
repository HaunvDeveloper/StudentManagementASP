using StudentManagementASP.Models;

namespace StudentManagementASP.ViewModels
{
    public class LecturerListViewModel
    {
        public List<Lecturer> Lecturers { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }

}
