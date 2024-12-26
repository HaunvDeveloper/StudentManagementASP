import pandas as pd
import os

def export_attendance(attendance_file='attendance_records/attendance.csv', export_path='attendance_records/exported_attendance.csv'):
    """
    Xuất dữ liệu điểm danh ra file CSV.
    
    Parameters:
    - attendance_file (str): Đường dẫn tới file điểm danh gốc.
    - export_path (str): Đường dẫn tới file xuất.
    """
    if os.path.exists(attendance_file):
        df = pd.read_csv(attendance_file)
        # Thực hiện bất kỳ xử lý nào nếu cần, ví dụ: lọc theo ngày
        df.to_csv(export_path, index=False)
        print(f"Đã xuất dữ liệu điểm danh tới {export_path}")
    else:
        print(f"Không tìm thấy file {attendance_file}")

if __name__ == "__main__":
    export_attendance()
