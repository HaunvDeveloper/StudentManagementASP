# StudentManagementASP

##  Giới thiệu

**StudentManagementASP** là một dự án ứng dụng web được xây dựng trên nền tảng ASP.NET Core (.NET 8), được thiết kế để quản lý toàn diện các hoạt động trong một cơ sở giáo dục (như trường đại học hoặc cao đẳng).

Dự án này bao gồm các tính năng quản lý sinh viên, giảng viên, chương trình học, các lớp học phần, và đặc biệt là một hệ thống điểm danh sử dụng công nghệ nhận diện khuôn mặt.

## 🌟 Tính năng chính

Hệ thống được phân chia thành ba phân hệ (Areas) chính với các vai trò và quyền hạn riêng biệt:

### 1. Admin (Quản trị viên)
* Quản lý toàn diện dữ liệu của hệ thống.
* Quản lý tài khoản và thông tin **Sinh viên** (Student).
* Quản lý tài khoản và thông tin **Giảng viên** (Lecturer).
* Quản lý **Khoa** (Department) và **Ngành học** (Major).
* Quản lý **Chương trình đào tạo** (Curriculum) và thêm/xóa môn học khỏi chương trình.
* Quản lý **Môn học** (Subject).
* Quản lý **Lớp học phần** (CourseClass), bao gồm cả việc nhập/xuất danh sách sinh viên và giảng viên từ file Excel.

### 2. Lecturer (Giảng viên)
* Xem danh sách các lớp học phần được phân công.
* Xem lịch dạy và thông tin chi tiết của từng buổi học.
* Xem danh sách sinh viên trong lớp học phần của mình.
* Thực hiện chức năng **Điểm danh** (Attendance), được hỗ trợ bởi hệ thống nhận diện khuôn mặt.

### 3. Student (Sinh viên)
* Xem thông tin cá nhân và chỉnh sửa thông tin liên hệ.
* Xem chương trình đào tạo của mình.
* Xem các môn học đã đăng ký và lịch học chi tiết.
* Đăng ký khuôn mặt để sử dụng cho hệ thống điểm danh.

### 4. Hệ thống Điểm danh Nhận diện Khuôn mặt
* Dự án tích hợp một thành phần Python để xử lý nhận diện khuôn mặt.
* **Đăng ký:** Sinh viên có thể chụp và tải lên 5 ảnh mẫu (`/Areas/Student/Views/Student/CreateFaceIdentify.cshtml`).
* **Xử lý:** Các script Python (`register.py`) xử lý và lưu trữ các đặc trưng khuôn mặt (dưới dạng file `.npy` trong `Scripts/Data/users`).
* **Điểm danh:** Giảng viên sử dụng chức năng điểm danh (`mark_attendance.py`) để hệ thống nhận diện sinh viên và ghi lại trạng thái tham dự.
* **Real-time:** Sử dụng **SignalR** (`AttendanceHub.cs`) để cập nhật trạng thái điểm danh trong thời gian thực.

## 🛠️ Công nghệ sử dụng

* **Framework:** .NET 8.0
* **Database:** SQL Server
* **ORM:** Entity Framework Core (sử dụng phương pháp Database First)
* **Kiến trúc:** ASP.NET Core MVC với Areas
* **Authentication:** ASP.NET Core Cookie Authentication
* **Real-time:** ASP.NET Core SignalR
* **Thư viện chính:**
    * `EPPlus`: Để đọc và ghi file Excel (nhập/xuất danh sách).
    * `DinkToPdf`: Để tạo và xuất file PDF.
* **Thành phần AI/ML:**
    * Python 3
    * Các thư viện (dựa trên `requirements.txt`): `opencv-python`, `numpy`, `face-recognition`.

## 🗃️ Cấu trúc Cơ sở dữ liệu (Models)

Dự án sử dụng `StudentManagementContext.cs` để quản lý các thực thể chính, bao gồm:

* `Student`, `Lecturer`, `User`: Quản lý thông tin người dùng.
* `Department`, `Major`: Quản lý cơ cấu tổ chức.
* `Curriculum`: Quản lý chương trình đào tạo.
* `Subject`, `Course`: Quản lý môn học.
* `CourseClass`: Quản lý các lớp học phần cụ thể.
* `StudentJoinClass`: Quản lý danh sách sinh viên trong một lớp học phần.
* `Lesson`: Quản lý các buổi học của một lớp học phần.
* `StudentJoinLesson`: Bảng ghi lại chi tiết trạng thái điểm danh của sinh viên trong từng buổi học.
* `Province`, `District`, `Ward`: Quản lý thông tin địa chỉ.

## 🚀 Cài đặt và Khởi chạy

1.  **Clone repository:**
    ```bash
    git clone <your-repository-url>
    cd StudentManagementASP
    ```
2.  **Cấu hình CSDL:**
    * Mở file `appsettings.json`.
    * Cập nhật chuỗi kết nối `StudentManagement` để trỏ đến cơ sở dữ liệu SQL Server của bạn.

3.  **Cài đặt môi trường Python:**
    * Đảm bảo bạn đã cài đặt Python 3.
    * Cài đặt các thư viện cần thiết:
        ```bash
        pip install -r StudentManagementASP/Compilers/requirements.txt
        ```

4.  **Chạy dự án:**
    ```bash
    dotnet run
    ```
