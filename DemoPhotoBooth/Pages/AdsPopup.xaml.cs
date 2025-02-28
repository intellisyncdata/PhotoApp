using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace DemoPhotoBooth.Pages
{
    /// <summary>
    /// Interaction logic for AdsPopup.xaml
    /// </summary>
    public partial class AdsPopup : Window
    {
        public bool UserWantsMoreTime { get; private set; }
        private DispatcherTimer autoCloseTimer;
        private int remainingTime = 10;

        public AdsPopup()
        {
            InitializeComponent();
            webView.Source = new Uri("https://photo-app.intellisyncdata.com"); // Thay bằng link của bạn

            webView.NavigationCompleted += (s, e) =>
            {
                webView.ExecuteScriptAsync(@"
                document.body.style.overflow = 'hidden'; 
                document.documentElement.style.overflow = 'hidden';
            ");
            };
            UserWantsMoreTime = false;

            // Khởi tạo bộ đếm thời gian
            autoCloseTimer = new DispatcherTimer();
            autoCloseTimer.Interval = TimeSpan.FromSeconds(1);
            autoCloseTimer.Tick += AutoCloseTimer_Tick;
            autoCloseTimer.Start();

            UpdateTimerUI();
        }

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
                this.Close(); // Đóng popup với kết quả "Không"
            }
        }

        private void UpdateTimerUI()
        {
            txtTimer.Text = $"{remainingTime}s";
        }
    }
}
