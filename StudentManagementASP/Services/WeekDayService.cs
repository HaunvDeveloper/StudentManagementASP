namespace StudentManagementASP.Services
{
    public class WeekDayService
    {
        public static DateTime FindNearestWeekDay(DateTime d, int weekDays)
        {
            DayOfWeek targetDay = (DayOfWeek)(((weekDays-1) % 7));

            // Nếu ngày hiện tại trùng với ngày mục tiêu, trả về ngay
            if (d.DayOfWeek == targetDay)
            {
                return d;
            }

            // Tính khoảng cách chiều xuôi (luôn dương)
            int forwardDistance = ((int)targetDay - (int)d.DayOfWeek + 7) % 7;

            // Trả về ngày kết quả
            return d.AddDays(forwardDistance);
        }
    }
}
