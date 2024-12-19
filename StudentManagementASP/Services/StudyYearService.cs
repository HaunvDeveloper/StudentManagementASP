using Microsoft.EntityFrameworkCore;
using StudentManagementASP.Models;
using StudentManagementASP.ViewModels;

namespace StudentManagementASP.Services
{
    public class StudyYearService
    {
        public static Semester? GetCurrentSemester(StudentManagementContext context)
        {
            var now = DateTime.Now;
            var semester = context.Semesters.AsNoTracking()
                .FirstOrDefault(x => now >= x.StartDate && now <= x.EndDate);
            return semester;
        }
    }
}
