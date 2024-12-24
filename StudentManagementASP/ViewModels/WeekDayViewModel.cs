namespace StudentManagementASP.ViewModels
{
    public class WeekDayViewModel
    {
        public WeekDayViewModel() 
        {
            Id = 1;
        }

        public WeekDayViewModel(int id)
        {
            Id = id;
        }

        public int Id { get; set; }

        public string Name
        {
            get
            {
                return this.ToString();
            }
        }

        public override string ToString()
        {
            switch (Id)
            {
                case 1:
                    return "Chủ Nhật";
                case 2:
                    return "Thứ Hai";
                case 3:
                    return "Thứ Ba";
                case 4:
                    return "Thứ Tư";
                case 5:
                    return "Thứ Năm";
                case 6:
                    return "Thứ Sáu";
                case 7:
                    return "Thứ Bảy";
                default:
                    return "Không hợp lệ";
            }
        }
    
        public static List<WeekDayViewModel> GetAll()
        {
            return new List<WeekDayViewModel>() { 
                new WeekDayViewModel(1),
                new WeekDayViewModel(2),
                new WeekDayViewModel(3),
                new WeekDayViewModel(4),
                new WeekDayViewModel(5),
                new WeekDayViewModel(6),
                new WeekDayViewModel(7),
            };
        }
    }

	public class Week
	{
		public int ThuTuTuan { get; set; }
		public DateTime NgayDauTuan { get; set; }
		public DateTime NgayCuoiTuan { get; set; }
	}
}
