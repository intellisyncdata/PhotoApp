using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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
        private SerialPort? serialPort;
        private readonly CommonDbDataContext _db;
        private readonly HttpClient _httpClient;
        public HomePage()
        {
            InitializeComponent();
            _db = new CommonDbDataContext();
            _httpClient = new HttpClient();
            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Close();
                serialPort.Dispose();
                serialPort = null;
            }

            var item = _db.PhotoApps.FirstOrDefault();
            if (item != null)
            {
                PhotoApps = new Models.DTOs.PhotoApp(item);
                DataContext = this;
            }
            var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backgrounds/bgmain.png");
            this.BgCustome.ImageSource = new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute));
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
                var info = _db.PhotoApps.AsNoTracking()?.FirstOrDefault();
                var checkcode = await CheckCode(info?.Code);
                if(!checkcode)
                {
                    NavigationService.Navigate(new CodeCheckPage());
                }
                else
                {
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
        private async Task<bool> CheckCode(string code)
        {
            try
            {
                if (_httpClient == null)
                {
                    MessageBox.Show("HTTP Client chưa được khởi tạo!", CommonMessages.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                if (string.IsNullOrEmpty(ApiUrl.ApiCheckCode))
                {
                    MessageBox.Show("API URL không hợp lệ!", CommonMessages.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                var requestData = new { code };
                var content = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync(ApiUrl.ApiCheckCode, content);
                string responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    if (JsonSerializer.Deserialize<Models.DTOs.PhotoApp>(responseContent) is { IsActive: true })
                        return true;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    var errorResponse = JsonSerializer.Deserialize<ApiErrorResponse>(responseContent);
                    if (errorResponse?.Message == "Photo app is already installed")
                        return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", CommonMessages.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        // Class để Deserialize phản hồi lỗi
        public class ApiErrorResponse
        {
            [JsonPropertyName("message")]
            public string Message { get; set; }

            [JsonPropertyName("error_code")]
            public string ErrorCode { get; set; }

            [JsonPropertyName("detail")]
            public string Detail { get; set; }
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
