using DemoPhotoBooth.Common;
using DemoPhotoBooth.DataContext;
using DemoPhotoBooth.Models;
using DemoPhotoBooth.Models.DTOs;
using DemoPhotoBooth.Pages.BackgroundPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace DemoPhotoBooth.Pages
{
    public partial class PaymentPage : Page
    {
        private SerialPort? serialPort;
        private decimal totalAmount = 0; // Biến lưu tổng tiền
        private string buffer = string.Empty;
        private decimal amountToPay = 70000; // Số tiền cần thanh toán
        private DispatcherTimer countdownTimer;
#if DEBUG
        private int remainingTime = 30; // Thời gian ban đầu (giây)
#else
        private int remainingTime = 30; // Thời gian ban đầu (giây)
#endif
        private bool isPopupShown = false;
        private bool isNextPage = false;
        public decimal TotalPrice { get; set; }
        public int Quantity { get; set; }
        public Layout Layout { get; set; }
        private readonly CommonDbDataContext _db;
        private int paymentId;
        private int quantity = 2;
        public List<BgLayout> Backgrounds { get; set; }
        public List<Layout> ListLayout { get; set; }
        public string colors { get; set; }
        public bool isApprove = false;

        public PaymentPage(decimal totalPrice, int quantity, Layout layout, List<BgLayout> backgrounds, string color, List<Layout> listLayouts)
        {
            _db = new CommonDbDataContext();
            InitializeComponent();
            TotalPrice = totalPrice;
            Quantity = quantity;
            Layout = layout;
            Backgrounds = backgrounds;
            ListLayout = listLayouts;
            colors = color;
            CloseSerialPort();
            btnContinue.IsEnabled = false;
            btnContinue.Opacity = 0.5;
            totalAmount = 0;
            InitializeSerialPort();
            SendCommand("5E");
        }

        private async void InitializeSerialPort()
        {
            try
            {
                if (serialPort != null && serialPort.IsOpen)
                {
                    LogMessage("Cổng COM đã mở.");
                    return;
                }

                serialPort = new SerialPort("COM2", 9600, Parity.None, 8, StopBits.One);
                serialPort.Encoding = System.Text.Encoding.ASCII;
                serialPort.DataReceived += SerialPort_DataReceived;
                serialPort.Open();
                LogMessage("TP70 đã được thiết lập và sẵn sàng.");
            }
            catch (UnauthorizedAccessException)
            {
                LogMessage("Lỗi: Không thể mở cổng COM (có thể cổng đang được sử dụng).");
            }
            catch (IOException)
            {
                LogMessage("Lỗi: Cổng COM không tồn tại hoặc bị ngắt kết nối.");
            }
            catch (Exception ex)
            {
                LogMessage("Lỗi kết nối: " + ex.Message);
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if(serialPort == null || !serialPort.IsOpen)
            {
                return;
            }
            else
            {
                try
                {
                    int bytesToRead = serialPort.BytesToRead;
                    byte[] receivedBytes = new byte[bytesToRead];
                    serialPort.Read(receivedBytes, 0, bytesToRead);

                    // Hiển thị dữ liệu dạng HEX để kiểm tra chính xác
                    string hexString = BitConverter.ToString(receivedBytes);
                    Dispatcher.Invoke(() => LogMessage("Dữ liệu nhận từ TP70 (HEX): " + hexString));

                    // Xử lý từng byte
                    foreach (byte b in receivedBytes)
                    {
                        Dispatcher.Invoke(() => LogMessage($"Byte nhận được: {b:X2} (Dec: {b})"));

                        // Kiểm tra các mã và cộng tiền
                        switch (b)
                        {
                            case 0x40:
                                totalAmount += 10000; // 10 ngàn đồng
                                SendCommand("02");
                                break;
                            case 0x41:
                                totalAmount += 20000; // 20 ngàn đồng
                                SendCommand("02");
                                break;
                            case 0x42:
                                totalAmount += 50000; // 50 ngàn đồng
                                SendCommand("02");
                                break;
                            case 0x43:
                                totalAmount += 100000; // 100 ngàn đồng
                                SendCommand("02");
                                break;
                        }

                        // Nếu cần gửi lệnh phản hồi, ví dụ gửi lệnh "02"
                        if (b >= 0x80 && b <= 0x8F)
                        {
                            SendCommand("02");
                        }

                        UpdateUI(paymentId);
                    }
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() => LogMessage("Lỗi đọc dữ liệu: " + ex.Message));
                }
            }
        }

        private void UpdateUI(int paymentId)
        {
            // Cập nhật giao diện số tiền đã nạp và số tiền cần thanh toán
            Dispatcher.Invoke(() =>
            {
                txtAmountToPay.Text = $"{amountToPay:N0} VNĐ";
                txtAmountDeposited.Text = $"{totalAmount:N0} VNĐ";

                // Hiển thị nút "Tiếp tục" nếu số tiền đã nạp đủ hoặc lớn hơn
                if (totalAmount >= amountToPay && isApprove)
                {
                    isNextPage = true;
                    btnContinue.IsEnabled = true;
                    btnContinue.Opacity = 1.0;
                }
            });
        }

        private void SendCommand(string command)
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                // Chuyển đổi chuỗi "02" thành byte thực tế và gửi qua cổng COM
                byte[] commandBytes = { Convert.ToByte(command, 16) }; // Chuyển "02" thành 0x02
                serialPort.Write(commandBytes, 0, commandBytes.Length);

                LogMessage("Đã gửi: " + BitConverter.ToString(commandBytes));
            }
            else
            {
                LogMessage("Cổng COM chưa mở.");
            }
        }


        private void BtnContinue_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Thanh toán thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void LogMessage(string message)
        {
            Dispatcher.Invoke(() => Console.WriteLine(message));
        }

        private void ShowPaymentPopup()
        {
            countdownTimer.Stop();

            // Hiển thị popup tùy chỉnh
            PaymentPopup popup = new PaymentPopup
            {
                Owner = Window.GetWindow(this), // Gắn popup với cửa sổ hiện tại
            };

            bool? result = popup.ShowDialog(); // Hiển thị popup và chờ kết quả

            if (result == true && popup.UserWantsMoreTime)
            {
                // Thêm 30 giây
                remainingTime = 30;
                isPopupShown = false;
                countdownTimer.Start();
                UpdateCountdownUI();
            }
            else
            {
                totalAmount = 0;
                CloseSerialPort();
                // Quay về trang chủ
                NavigateToHomePage(this, new RoutedEventArgs());
            }
        }
        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            if (isNextPage)
            {
                countdownTimer.Stop();
            }

            if (remainingTime > 0)
            {
                remainingTime--;
                UpdateCountdownUI();
            }
            else if (!isPopupShown)
            {
                // Hiển thị popup khi hết thời gian
                ShowPaymentPopup();
                isPopupShown = true;
            }
        }

        private void UpdateCountdownUI()
        {
            txtCountdown.Text = $"{remainingTime}s";
        }

        private void NavigateToHomePage(object sender, RoutedEventArgs e)
        {
            totalAmount = 0;
            CloseSerialPort();
            NavigationService?.Navigate(new HomePage());
        }

        private void NavigateToPreviousPage(object sender, RoutedEventArgs e)
        {
            totalAmount = 0;
            CloseSerialPort();
            NavigationService?.Navigate(new BackgroundPage(Layout, colors, ListLayout));
        }

        private void NavigateToCameraMode(object sender, RoutedEventArgs e)
        {
            // Điều hướng tới trang CameraMode
            CompleteTransactionPayment(paymentId, totalAmount);
            totalAmount = 0;
            CloseSerialPort();
            NavigationService?.Navigate(new CameraMode(Layout, ListLayout));
        }

        private int CreateTransactionPayment(int layoutId, int quantity)
        {
            var info = _db.PhotoApps.AsNoTracking()?.FirstOrDefault();
            string token = info.Token;
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string apiUrlTemplate = ApiUrl.ApiCreateTransactionPayment;
                    string apiUrl = string.Format(apiUrlTemplate, info.Id);

                    var data = new CalculatePaymentPayload
                    {
                        LayoutId = layoutId,
                        Quantity = quantity,
                        PaymentProvider = "cash"
                    };
                    var json = JsonSerializer.Serialize(data);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(token);
                    HttpResponseMessage response = client.PostAsync(apiUrl, content).GetAwaiter().GetResult(); ;
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult(); ;
                        var responseDict = JsonSerializer.Deserialize<Dictionary<string, int>>(responseContent);
                        paymentId = responseDict?.GetValueOrDefault("payment_id") ?? 0;

                        return paymentId;

                    }
                    else
                    {
                        MessageBox.Show($"Lỗi khi gửi API", CommonMessages.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi gửi API: {ex.Message}", CommonMessages.Error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return -1;
        }

        private async Task<string> CompleteTransactionPayment(int paymentId, decimal totalAmount)
        {
            var info = _db.PhotoApps.AsNoTracking()?.FirstOrDefault();
            string token = info.Token;
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string apiUrlTemplate = ApiUrl.ApiCompleteTransactionPayment;
                    string apiUrl = string.Format(apiUrlTemplate, info.Id);

                    var data = new CalculatePaymentPayload
                    {
                        PaymentId = paymentId,
                        PaymentProvider = "cash",
                        TotalPrice = totalAmount
                    };
                    var json = JsonSerializer.Serialize(data);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(token);
                    HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseContent);

                        // Serialize the dictionary back to a string (JSON format)
                        string resultJson = JsonSerializer.Serialize(result);
                        return resultJson;

                    }
                    else
                    {
                        MessageBox.Show($"Lỗi khi gửi API", CommonMessages.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi gửi API: {ex.Message}", CommonMessages.Error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return string.Empty;
        }

        private void PaymentModeChanged(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                if (button.Name == "btnCash")
                {
                    PaymentContent.Visibility = Visibility.Visible;
                    PaymentOnlineContent.Visibility = Visibility.Collapsed;
                    btnOnline.Opacity = 0.5;
                    btnCash.Opacity = 1;
                }
                else if (button.Name == "btnOnline")
                {
                    PaymentContent.Visibility = Visibility.Collapsed;
                    PaymentOnlineContent.Visibility = Visibility.Visible;
                    btnOnline.Opacity = 1;
                    btnCash.Opacity = 0.5;

                }
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            amountToPay = TotalPrice;
            txtCountdown.Visibility = Visibility.Collapsed;
            bgCountDown.Visibility = Visibility.Collapsed;
            txtAmountToPay.Text = $"{amountToPay:N0} VNĐ";
            SendCommand("5c");
        }

        private void IncreaseQuantity(object sender, RoutedEventArgs e)
        {
            quantity += 2;
            UpdateAmount();
        }

        private void DecreaseQuantity(object sender, RoutedEventArgs e)
        {
            if (quantity > 2)
            {
                quantity -= 2;
                UpdateAmount();
            }
        }

        private void UpdateAmount()
        {
            txtQuantity.Text = quantity.ToString();
            txtAmountToPay.Text = (quantity / 2 * TotalPrice).ToString("N0") + " VND";
            amountToPay = (quantity / 2 * TotalPrice);
        }


        private void AcceptPayment(object sender, EventArgs e)
        {
            UpdateAmount();
            totalAmount = 0;
            InitializeSerialPort();
            SendCommand("02");
            txtCountdown.Visibility = Visibility.Visible;
            bgCountDown.Visibility = Visibility.Visible;
            NavigateToBack.IsEnabled = false;
            NavigateToBack.Opacity = 0.5;
            countdownTimer = new DispatcherTimer();
            countdownTimer.Interval = TimeSpan.FromSeconds(1);
            countdownTimer.Tick += CountdownTimer_Tick;
            countdownTimer.Start();
            UpdateCountdownUI();
            LeftPayment.IsEnabled = false;
            btnAccept.IsEnabled = false;
            btnAccept.Opacity = 0.5;
            btnQuantity.Visibility = Visibility.Hidden;
            btnAddQuantity.Visibility = Visibility.Hidden;
            RightPayment.Background = new SolidColorBrush(Colors.White);
            LeftPayment.Background = new SolidColorBrush(Colors.Gray);
            paymentId = CreateTransactionPayment(Layout.Id, quantity);
            isApprove = true;
            UpdateUI(paymentId);
            var layout = _db.LayoutApp.FirstOrDefault();
            if (layout != null)
            {
                layout.PaymentId = paymentId;
                layout.PrintQuantity = quantity;
                _db.Entry(layout).State = EntityState.Modified;
                _db.SaveChanges();
            }
        }

        private void CloseSerialPort()
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Close();
                serialPort.Dispose();
                serialPort = null;
                LogMessage("Cổng COM đã đóng.");
            }
        }
    }
}
