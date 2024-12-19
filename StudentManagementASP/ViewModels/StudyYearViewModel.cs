using StudentManagementASP.Models;

namespace StudentManagementASP.ViewModels
{
    public class StudyYearViewModel
    {
        public int Id { get; set; }

        public int Number { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime ExpireDate { get; set; }

        public int? StartYearId { get; set; }

        public int? EndYearId { get; set; }

        public virtual List<StudyYearDetailViewModel>? StudyYearDetails { get; set; }

        public SemesterViewModel CurrentSemester { get; set; } = new SemesterViewModel();
    }


    public class StudyYearDetailViewModel
    {
        public int Id { get; set; }

        public int StartYear { get; set; }

        public int EndYear { get; set; }

        public virtual List<SemesterViewModel> semester { get; set; } = new List<SemesterViewModel>();
    }

    public class SemesterViewModel
    {
        public int Id { get; set; }

        public string Code { get; set; } = null!;

        public string Name { get; set; } = null!;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public int SchoolYearDetailId { get; set; }

        public virtual StudyYearDetailViewModel StudyYearDetailViewModel { get; set; } = new StudyYearDetailViewModel();
    }
}
