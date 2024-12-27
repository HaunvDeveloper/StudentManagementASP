using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
namespace StudentManagementASP.Hubs
{
    public class AttendanceHub : Hub
    {
        public async Task SendAttendance(string studentId, string status)
        {
            // Gửi thông báo tới tất cả client
            await Clients.All.SendAsync("ReceiveAttendance", studentId, status);
        }
    }

}
