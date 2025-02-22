using System.Diagnostics;
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

        private async void CheckCode_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CodeInput.Text))
            {
                MessageBox.Show("Please enter some code to check!", CommonMessages.Warning, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            else
            {
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

        private async void CheckUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckVersion();

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
