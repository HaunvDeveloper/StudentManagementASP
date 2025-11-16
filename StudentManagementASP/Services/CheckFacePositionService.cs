using System.Diagnostics;

namespace StudentManagementASP.Services
{
    public class CheckFacePositionService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CheckFacePositionService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<string> CheckFacePosition(string imageBase64)
        {
            try
            {
                if (string.IsNullOrEmpty(imageBase64))
                {
                    return "Error: No image data provided.";
                }

                // Giải mã ảnh từ base64 và lưu vào thư mục tạm
                var base64Data = imageBase64.Replace("data:image/png;base64,", "");
                var imageBytes = Convert.FromBase64String(base64Data);

                // Tạo thư mục TempImages nếu chưa tồn tại
                var tempFolder = Path.Combine( "wwwroot", "TempImages");
                Directory.CreateDirectory(tempFolder); // Tạo thư mục nếu chưa tồn tại

                // Tạo đường dẫn tạm thời cho ảnh
                var tempImagePath = Path.Combine(tempFolder, "temp.png");
                File.WriteAllBytes(tempImagePath, imageBytes);

                // Tạo đường dẫn tương đối cho tệp Python
                var scriptPath = Path.Combine("Scripts", "Python", "check.py");

                // Gọi script Python
                var result = await RunPythonScript(scriptPath, tempImagePath);

                // Trả kết quả
                return result;
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }


        private async Task<string> RunPythonScript(string scriptPath, string imagePath)
        {
            // Bao quanh đường dẫn bằng dấu nháy kép để đảm bảo không bị lỗi vì dấu cách trong đường dẫn
            var start = new ProcessStartInfo
            {
                FileName = Path.Combine("Compilers", "python", "python.exe"),
                Arguments = $"\"{scriptPath}\" \"{imagePath}\"",  // Đảm bảo đường dẫn được bao quanh bằng dấu nháy kép
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,  // Đọc lỗi nếu có
                CreateNoWindow = true
            };

            using (var process = Process.Start(start))
            {
                using (var reader = process.StandardOutput)
                using (var errorReader = process.StandardError)
                {
                    var output = await reader.ReadToEndAsync();
                    var errorOutput = await errorReader.ReadToEndAsync();

                    if (!string.IsNullOrEmpty(errorOutput))
                    {
                        return $"Error: {errorOutput}\nOutput: {output}";
                    }

                    return output;
                }
            }
        }

    }
}
