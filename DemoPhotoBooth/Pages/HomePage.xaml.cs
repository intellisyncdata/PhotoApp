using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DemoPhotoBooth.Common;
using DemoPhotoBooth.DataContext;
using DemoPhotoBooth.Models;
using DemoPhotoBooth.Models.DTOs;
using DemoPhotoBooth.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DemoPhotoBooth.Pages
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : Page
    {
        public Models.DTOs.PhotoApp PhotoApps { get; set; }
        public List<Layout> Layouts { get; set; }
        private readonly CommonDbDataContext _db;
        public HomePage()
        {
            InitializeComponent();
            _db = new CommonDbDataContext();
            var item = _db.PhotoApps.FirstOrDefault();
            if (item != null)
            {
                PhotoApps = new Models.DTOs.PhotoApp(item);
                DataContext = this;
            }
        }

        //public HomePage(Models.DTOs.PhotoApp photoApps)
        //{
        //    InitializeComponent();

        //    PhotoApps = photoApps;

        //    DataContext = this;
        //}

        private async void StartApi_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool isCameraConnected =  CheckCamera();
                bool isMoneyCheckerConnected = CheckMoneyChecker();
                bool isPrinterConnected = CheckPrinter();

                if (!isCameraConnected || !isMoneyCheckerConnected || !isPrinterConnected)
                {
                    await SendNotificationAsync(isCameraConnected, isMoneyCheckerConnected, isPrinterConnected);
                    MessageBox.Show($"Lỗi: Không tìm thấy camera {isCameraConnected} money {isMoneyCheckerConnected} printer {isPrinterConnected}", CommonMessages.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var info = _db.PhotoApps.AsNoTracking()?.FirstOrDefault();
                var layouts = await GetLayout(PhotoApps, info.Token);

                if (layouts != null || layouts?.Count > 0)
                {
                    List<Task> downloadTasks = new List<Task>();

                    foreach (var layout in layouts)
                    {
                        string imageUrl = layout.ImageUrl;

                        if (!string.IsNullOrEmpty(imageUrl))
                        {
                            downloadTasks.Add(DownloadImageAsync(imageUrl));
                        }
                    }

                    await Task.WhenAll(downloadTasks);

                    NavigationService.Navigate(new LayoutPage(layouts));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi kiểm tra thiết bị: {ex.Message}", CommonMessages.Error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DownloadImageAsync(string imageUrl)
        {
            try
            {
                using HttpClient client = new HttpClient();

                string layoutName = System.IO.Path.GetFileName(new Uri(imageUrl).LocalPath);
                string savePath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Layouts", layoutName);

                if (!File.Exists(savePath)) // Kiểm tra nếu file chưa tồn tại để tránh tải lại
                {
                    byte[] imageBytes = await client.GetByteArrayAsync(imageUrl);
                    Directory.CreateDirectory(System.IO.Path.GetDirectoryName(savePath)); // Đảm bảo thư mục tồn tại
                    await File.WriteAllBytesAsync(savePath, imageBytes);

                    Console.WriteLine($"Tải xuống thành công: {layoutName}");
                }
                else
                {
                    Console.WriteLine($"Bỏ qua: {layoutName} đã tồn tại.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi tải ảnh {imageUrl}: {ex.Message}");
            }
        }

        /// <summary>
        /// Kiểm tra Camera có kết nối không
        /// </summary>
        public static bool CheckCamera()
        {
            try
            {
                // Chạy PowerShell để lấy danh sách thiết bị kết nối
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = "Get-PnpDevice | Where-Object { $_.FriendlyName -match 'Camera|Webcam|Canon|EOS|R100' } | Select-Object -ExpandProperty FriendlyName",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                List<string> foundDevices = new List<string>();

                // Tách các dòng output để lấy tên thiết bị
                foreach (var line in output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                {
                    Console.WriteLine($"Found Device: {line}");
                    foundDevices.Add(line);
                }

                // Kiểm tra danh sách thiết bị xem có Canon R100 hay không
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
                    HttpResponseMessage response = await client.PostAsync(apiUrl,content);
                    return response.IsSuccessStatusCode;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi gửi API: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private async Task<List<Layout>> GetLayout(Models.DTOs.PhotoApp photoApp, string token)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string apiUrlTemplate = ApiUrl.ApiGetLayout;
                    string apiUrl = string.Format(apiUrlTemplate, photoApp.Id);
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(token);
                    HttpResponseMessage response = await client.GetAsync(apiUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        List<Layout> result = JsonSerializer.Deserialize<List<Layout>>(responseContent);
                        return result ?? new List<Layout>();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi gửi API: {ex.Message}", CommonMessages.Error, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return new List<Layout>();
        }
    }
}
