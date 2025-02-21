using System;
using System.Collections.Generic;
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
                //bool isCameraConnected = CheckCamera();
                //bool isMoneyCheckerConnected = CheckMoneyChecker();
                //bool isPrinterConnected = CheckPrinter();

                //if (!isCameraConnected)
                //{
                //    await SendNotificationAsync("Lỗi: Không tìm thấy camera!");
                //    MessageBox.Show("Lỗi: Không tìm thấy camera!", CommonMessages.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                //    return;
                //}

                //if (!isMoneyCheckerConnected)
                //{
                //    await SendNotificationAsync("Lỗi: Không tìm thấy máy kiểm tra tiền!");
                //    MessageBox.Show("Lỗi: Không tìm thấy máy kiểm tra tiền!", CommonMessages.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                //    return;
                //}

                //if (!isPrinterConnected)
                //{
                //    await SendNotificationAsync("Lỗi: Không tìm thấy máy in!");
                //    MessageBox.Show("Lỗi: Không tìm thấy máy in!", CommonMessages.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                //    return;
                //}
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
        private bool CheckCamera()
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Description LIKE '%Camera%' OR Description LIKE '%Webcam%'"))
            {
                return searcher.Get().Count > 0;
            }
        }

        /// <summary>
        /// Kiểm tra máy kiểm tra tiền (qua cổng Serial)
        /// </summary>
        private bool CheckMoneyChecker()
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_SerialPort"))
            {
                return searcher.Get().Count > 0;
            }
        }

        /// <summary>
        /// Kiểm tra máy in có kết nối không
        /// </summary>
        private bool CheckPrinter()
        {
            string printerName = "DS-RX1";
            using (var searcher = new ManagementObjectSearcher($"SELECT * FROM Win32_Printer WHERE Name = '{printerName}'"))
            {
                return searcher.Get().Count > 0;
            }
        }

        private async Task<bool> SendNotificationAsync(string message)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string apiUrl = ApiUrl.ApiSendMessage;
                    var data = new { message = message, MachineCode = PhotoApps.Code };
                    var json = System.Text.Json.JsonSerializer.Serialize(data);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

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
