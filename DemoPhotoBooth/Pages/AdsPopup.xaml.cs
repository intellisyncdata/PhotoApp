using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;



namespace DemoPhotoBooth.Pages
{
    /// <summary>
    /// Interaction logic for AdsPopup.xaml
    /// </summary>
    public partial class AdsPopup : Window, IDisposable
    {
        public bool UserWantsMoreTime { get; private set; }
        private DispatcherTimer autoCloseTimer;
        private int remainingTime = 10;

        public AdsPopup()
        {
            InitializeComponent();
            webView.Source = new Uri("https://photo-app.intellisyncdata.com"); // Thay bằng link của bạn

            webView.NavigationCompleted += WebView_NavigationCompleted;

            UserWantsMoreTime = false;

            // Khởi tạo bộ đếm thời gian
            autoCloseTimer = new DispatcherTimer();
            autoCloseTimer.Interval = TimeSpan.FromSeconds(1);
            autoCloseTimer.Tick += AutoCloseTimer_Tick;
            autoCloseTimer.Start();

            UpdateTimerUI();

            // Đăng ký sự kiện đóng để giải phóng tài nguyên
            this.Closed += Window_Closed;
        }

        [DllImport("kernel32.dll")]
        static extern bool SetProcessWorkingSetSize(IntPtr proc, int min, int max);

        private void AutoCloseTimer_Tick(object sender, EventArgs e)
        {
            if (remainingTime > 0)
            {
                remainingTime--;
                UpdateTimerUI();
            }
            else
            {
                autoCloseTimer.Stop();
                this.Close(); // Đóng popup
            }
        }

        private void UpdateTimerUI()
        {
            txtTimer.Text = $"{remainingTime}s";
        }

        private void WebView_NavigationCompleted(object sender, EventArgs e)
        {
            webView.ExecuteScriptAsync(@"
                document.body.style.overflow = 'hidden'; 
                document.documentElement.style.overflow = 'hidden';
            ");
        }

        // Giải phóng tài nguyên khi cửa sổ đóng
        private void Window_Closed(object sender, EventArgs e)
        {
            Dispose();
        }

        public void Dispose()
        {
            if (autoCloseTimer != null)
            {
                autoCloseTimer.Stop();
                autoCloseTimer.Tick -= AutoCloseTimer_Tick;
                autoCloseTimer = null;
            }

            if (webView != null)
            {
                webView.NavigationCompleted -= WebView_NavigationCompleted;
                webView.Dispose();
                webView = null;
            }

            this.Content = null; // Xóa UI

            // Thu gom bộ nhớ
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            // Giảm Private Bytes
            SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
        }
    }
}

