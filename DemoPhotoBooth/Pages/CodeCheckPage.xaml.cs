using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Resources;
using DemoPhotoBooth.Common;
using DemoPhotoBooth.DataContext;
using DemoPhotoBooth.Models;
using DemoPhotoBooth.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace DemoPhotoBooth.Pages
{
    public partial class CodeCheckPage : Page
    {
        private readonly HttpClient _httpClient;
        public PhotoApp PhotoApps { get; set; } = new PhotoApp();
        private readonly CommonDbDataContext _db;
        public CodeCheckPage()
        {
            InitializeComponent();
#if DEBUG
            CodeInput.Text = "ZR3yT2POsAE";
#endif
            CheckVersion();
            _httpClient = new HttpClient();
            _db = new CommonDbDataContext();
            if (_db != null)
            {
                CheckCodeDb();
            }
        }

        private void CheckCodeDb()
        {
            var photoApp = _db.PhotoApps.AsNoTracking().FirstOrDefault();

            CodeInput.Text = photoApp?.Code ?? "PHOTOAPP001";
        }

        private async Task<bool> CheckDevice()
        {
            bool isCameraConnected = CheckCamera();
            bool isMoneyCheckerConnected = CheckMoneyChecker();
            bool isPrinterConnected = CheckPrinter();

            if (!isCameraConnected || !isMoneyCheckerConnected || !isPrinterConnected)
            {
                await SendNotificationAsync(isCameraConnected, isMoneyCheckerConnected, isPrinterConnected);
                MessageBox.Show($"Lỗi: Không tìm thấy camera {isCameraConnected} money {isMoneyCheckerConnected} printer {isPrinterConnected}", CommonMessages.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        public static bool CheckCamera()
        {
            try
            {
                // Chạy PowerShell để lấy danh sách thiết bị đang kết nối
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = "Get-CimInstance Win32_PnPEntity | Where-Object { $_.Name -match 'Camera|Webcam|Canon|EOS|R100' } | Select-Object -ExpandProperty Name",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                List<string> foundDevices = new List<string>();

                foreach (var line in output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                {
                    Console.WriteLine($"Found Device: {line}");
                    foundDevices.Add(line);
                }

                // Kiểm tra xem có thiết bị mong muốn không
                foreach (var device in foundDevices)
                {
                    if (device.Contains("Canon", StringComparison.OrdinalIgnoreCase) ||
                        device.Contains("EOS", StringComparison.OrdinalIgnoreCase) ||
                        device.Contains("R100", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking camera: {ex.Message}");
            }

            return false;
        }


        /// <summary>
        /// Kiểm tra máy kiểm tra tiền (qua cổng Serial)
        /// </summary>
        public static bool CheckMoneyChecker()
        {
            string targetPort = "COM2";

            // Chạy lệnh "mode" để lấy danh sách các cổng COM
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/c mode",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            // Kiểm tra xem output có chứa cổng COM mong muốn không
            return output.Contains(targetPort, StringComparison.OrdinalIgnoreCase);
        }


        /// <summary>
        /// Kiểm tra máy in có kết nối không
        /// </summary>
        private bool CheckPrinter()
        {
            string printerName = "DS-RX1";
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"Get-Printer | Where-Object {{$_.Name -eq '{printerName}'}}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            // Nếu output chứa tên máy in => máy in tồn tại
            return !string.IsNullOrWhiteSpace(output);
        }

        private async Task<bool> SendNotificationAsync(bool isCameraConnected, bool isMoneyCheckerConnected, bool isPrinterConnected)
        {
            try
            {
                var photoApp = _db.PhotoApps.FirstOrDefault();
                var token = photoApp.Token;
                string apiUrl = string.Format(ApiUrl.ApiSendMessage, photoApp.Id);

                using (HttpClient client = new HttpClient())
                {
                    var data = new
                    {
                        camera = isCameraConnected,
                        bill_acceptor = isMoneyCheckerConnected,
                        printer = isPrinterConnected,
                        printer_paper_count = 0
                    };

                    var json = JsonSerializer.Serialize(data);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(token);
                    HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                    return response.IsSuccessStatusCode;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi gửi API: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private async void CheckCode_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CodeInput.Text))
            {
                MessageBox.Show("Please enter some code to check!", CommonMessages.Warning, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            else
            {
#if !DEBUG
                if (!await CheckDevice())
                {
                    MessageBox.Show("Không tìm thấy thiết bị!", CommonMessages.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
#endif
                var photoApp = _db.PhotoApps.AsNoTracking().FirstOrDefault();

                if (photoApp != null && photoApp.Code == CodeInput.Text)
                {
                    NavigationService.Navigate(new HomePage());
                }
                else
                {
                    btnCheckCode.IsEnabled = btnResetPrinter.IsEnabled = CodeInput.IsEnabled = false;
                    PhotoApp result = await CheckCodeAsync(CodeInput.Text);

                    if (result != null)
                    {
                        SavePhotoApp(result);
                        InitPrintInformation(result.Id);

                        NavigationService.Navigate(new HomePage());
                    }
                    else
                    {
                        MessageBox.Show("Invalid code. Please try again.", CommonMessages.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    btnCheckCode.IsEnabled = btnResetPrinter.IsEnabled = CodeInput.IsEnabled = true;
                }
            }
        }

        private async void CheckVersion()
        {
            Uri resourceUri = new Uri("pack://application:,,,/Text/Version.txt");
            string VersionCheckUrl = "https://photo-app-api.intellisyncdata.com/api/v1/apps/check_version?version=";
            StreamResourceInfo resourceInfo = Application.GetResourceStream(resourceUri);

            if (resourceInfo != null)
            {
                using (StreamReader reader = new StreamReader(resourceInfo.Stream))
                {
                    string version = await reader.ReadToEndAsync();
                    using (HttpClient client = new HttpClient())
                    {
                        HttpResponseMessage response = await client.GetAsync(VersionCheckUrl + version);
                        if (response.IsSuccessStatusCode)
                        {
                            btnUpdate.IsEnabled = true;
                            btnUpdate.Opacity = 1;
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Không tìm thấy tệp Version.txt", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DownloadVersion()
        {
            Uri resourceUri = new Uri("pack://application:,,,/Text/Version.txt");
            string VersionCheckUrl = "https://photo-app-api.intellisyncdata.com/api/v1/apps/check_version?version=";
            StreamResourceInfo resourceInfo = Application.GetResourceStream(resourceUri);

            if (resourceInfo != null)
            {
                using (StreamReader reader = new StreamReader(resourceInfo.Stream))
                {
                    string version = await reader.ReadToEndAsync();
                    using (HttpClient client = new HttpClient())
                    {
                        HttpResponseMessage response = await client.GetAsync(VersionCheckUrl + version);
                        if (response.IsSuccessStatusCode)
                        {
                            btnUpdate.IsEnabled = true;
                            btnUpdate.Opacity = 1;
                            //lấy url
                            //tải file
                            //update version.txt
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Không tìm thấy tệp Version.txt", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private async void CheckUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DownloadVersion();

                // URL API để tải file
                string apiUrl = "https://example.com/api/download";
                string filePath = @"C:\Path\To\Your\Exe\File.exe"; // Đường dẫn file exe hiện tại

                // Tải file mới về
                await DownloadFileAsync(apiUrl, filePath);

                // Khởi động file exe sau khi cập nhật
                StartFile(filePath);

                MessageBox.Show("Update completed successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private async Task DownloadFileAsync(string url, string filePath)
        {
            using (HttpClient client = new HttpClient())
            {
                // Tải dữ liệu từ API
                byte[] fileBytes = await client.GetByteArrayAsync(url);

                // Ghi đè file cũ
                await File.WriteAllBytesAsync(filePath, fileBytes);
            }
        }

        private void StartFile(string filePath)
        {
            // Khởi động file exe sau khi đã tải xong
            Process.Start(filePath);
        }

        private void InitPrintInformation(uint id)
        {
            try
            {
                var isExisted = _db.CommonDatas.Any(x => x.PrinterId == 1);
                if (isExisted)
                {
                    return;
                }

                _db.CommonDatas.Add(new Models.Entities.CommonData
                {
                    PrintCount = 0,
                    PrinterId = (int)id
                });

                _db.SaveChanges();
            }
            catch (Exception ex)
            {

            }
        }

        private void SavePhotoApp(PhotoApp result)
        {
            try
            {
                CheckPhotoApp();
                var ePhotoApp = new DemoPhotoBooth.Models.Entities.PhotoApp(result);
                _db.PhotoApps.Add(ePhotoApp);

                _db.SaveChanges();
            }
            catch (Exception ex)
            {

            }
        }

        private void CheckPhotoApp()
        {
            var isExisted = _db.PhotoApps.Any();

            if (isExisted)
            {
                foreach (var item in _db.PhotoApps)
                {
                    _db.PhotoApps.Remove(item);
                }

                _db.SaveChanges(true);
            }
        }

        private async void ResetPrinter_Click(object sender, RoutedEventArgs e)
        {
            var info = _db.CommonDatas.AsNoTracking()?.FirstOrDefault();
            if (info == null)
            {
                MessageBox.Show("Không tìm thấy Thông tin!", CommonMessages.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Nếu giá trị >= 650, gọi API gửi tin nhắn
            if (info.PrintCount >= 650)
            {
                btnCheckCode.IsEnabled = btnResetPrinter.IsEnabled = CodeInput.IsEnabled = false;

                PhotoApp result = await CheckCodeAsync(CodeInput.Text); // Validate PCCode

                if (result != null)
                {
                    bool isSent = await SendNotificationAsync(result.Id, info.PrintCount, result.Token);
                    if (isSent)
                    {
                        info.PrintCount = 0;
                        _db.CommonDatas.Update(info);
                        _db.SaveChanges();

                        MessageBox.Show("Đã Reset Đếm Giấy Máy In.", CommonMessages.Alert, MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Gửi thông báo thất bại!", CommonMessages.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Invalid code. Please try again.", CommonMessages.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                }
                btnCheckCode.IsEnabled = btnResetPrinter.IsEnabled = CodeInput.IsEnabled = true;
            }
        }

        private async Task<PhotoApp> CheckCodeAsync(string code)
        {
            try
            {
                var requestData = new { code = code };
                string json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                string apiUrl = ApiUrl.ApiCheckCode;
                HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    PhotoApp result = JsonSerializer.Deserialize<PhotoApp>(responseContent);
                    return result ?? null;
                }
                else
                {
                    MessageBox.Show($"Failed to check code. Error: {response.StatusCode}", CommonMessages.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error occurred: {ex.Message}", CommonMessages.Error, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return null;
        }

        private async Task<bool> SendNotificationAsync(uint id, int count, string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                MessageBox.Show($"Không thể reset máy in.", CommonMessages.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string apiUrl = string.Format(ApiUrl.ApiSendMessage, id);

                    var data = new PingPayload
                    {
                        PrinterCount = count
                    };
                    var json = System.Text.Json.JsonSerializer.Serialize(data);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(token);
                    HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                    return response.IsSuccessStatusCode;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi gửi API: {ex.Message}", CommonMessages.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private string GetProjectPath()
        {
            string currentPath = Directory.GetCurrentDirectory();

            // Tìm thư mục gốc "PhotoBoothProject"
            while (!string.IsNullOrEmpty(currentPath) && !Directory.Exists(Path.Combine(currentPath, "PhotoBoothProject")))
            {
                currentPath = Directory.GetParent(currentPath)?.FullName ?? "";
            }

            // Nếu không tìm thấy thư mục dự án, trả về null
            if (string.IsNullOrEmpty(currentPath))
            {
                return null;
            }

            // Trả về đường dẫn đầy đủ đến thư mục cần kiểm tra
            return Path.Combine(currentPath, "PhotoBoothProject", "DemoPhotoBooth", "DemoPhotoBooth", "Text");
        }

    }
}
