using StudentManagementASP.ViewModels;

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

		public static List<Week> CreateListWeek(DateTime startDate, DateTime endDate)
		{
			List<Week> danhSachTuan = new List<Week>();

			// Đưa startDate về ngày thứ 2 đầu tiên
			DateTime current = startDate;
			while (current.DayOfWeek != DayOfWeek.Monday)
			{
				current = current.AddDays(1);
			}

			int thuTuTuan = 1;
			while (current <= endDate)
			{
				DateTime ngayDauTuan = current;
				DateTime ngayCuoiTuan = current.AddDays(6);

				if (ngayDauTuan > endDate)
					break;

				if (ngayCuoiTuan > endDate)
					ngayCuoiTuan = endDate;

				danhSachTuan.Add(new Week
				{
					ThuTuTuan = thuTuTuan,
					NgayDauTuan = ngayDauTuan,
					NgayCuoiTuan = ngayCuoiTuan
				});

				thuTuTuan++;
				current = current.AddDays(7); 
			}

			return danhSachTuan;
		}

		public static int TimTuanHienTai(List<Week> danhSachTuan, DateTime ngayHienTai)
		{
			foreach (var tuan in danhSachTuan)
			{
				if (ngayHienTai >= tuan.NgayDauTuan && ngayHienTai <= tuan.NgayCuoiTuan)
				{
					return tuan.ThuTuTuan;
				}
			}
			return -1; // Trả về -1 nếu không tìm thấy tuần
		}
	}
}
