using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Windows;
using DemoPhotoBooth.Common;
using DemoPhotoBooth.DataContext;
using System.Net.Http.Headers;
using DemoPhotoBooth.Pages.Preview;
using DemoPhotoBooth.Pages;
using System.IO.Ports;
using System.ComponentModel;

namespace DemoPhotoBooth
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly CommonDbDataContext _db;
        private readonly HttpClient _httpClient;
        private SerialPort? serialPort;
        public MainWindow()
        {
            InitializeComponent();
            _db = new CommonDbDataContext();
            _httpClient = new HttpClient();

            MainFrame.Navigate(new CodeCheckPage());
        }

        private async Task DeactivePhotoApp()
        {
            try
            {
                string apiUrl = ApiUrl.ApiDeactive;
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(ApiUrl.DefaultToken);
                var content = new StringContent(JsonSerializer.Serialize(new { status = "not_yet" }), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _httpClient.PutAsync(apiUrl, content);
            }
            catch (Exception ex)
            {

            }
        }

        private void ClearCommonData()
        {
            foreach (var item in _db.PhotoApps)
            {
                _db.PhotoApps.Remove(item);
            }

            _db.SaveChanges();
        }

        private void ClearLayoutApp()
        {
            var isExisted = _db.LayoutApp.Any();

            if (isExisted)
            {
                foreach (var item in _db.LayoutApp)
                {
                    _db.LayoutApp.Remove(item);
                }
                _db.SaveChanges();
            }
        }

        private void ClosePort()
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Close();
                serialPort.Dispose();
                serialPort = null;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                //Task task1 = Task.Factory.StartNew(() => DeactivePhotoApp());
                ClearLayoutApp();
                ClosePort();
                Environment.Exit(0);
                //Task task2 = Task.Factory.StartNew(() => ClearCommonData());
                //Task.WaitAll(task1, task2);
            }
            catch (Exception ex)
            {

            }
        }
    }
}