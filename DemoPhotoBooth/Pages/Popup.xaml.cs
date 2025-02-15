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
    /// Interaction logic for Popup.xaml
    /// </summary>
    public partial class Popup : Window
    {
        public bool UserWantsMoreTime { get; private set; }
        private DispatcherTimer autoCloseTimer;
        private int remainingTime = 10;

        public Popup()
        {
            InitializeComponent();
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
                this.DialogResult = false; // Đóng popup với kết quả "Không"
            }
        }

        private void UpdateTimerUI()
        {
            txtTimer.Text = $"({remainingTime} giây sau trở về trang chủ)";
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            UserWantsMoreTime = true;
            autoCloseTimer.Stop(); // Dừng bộ đếm thời gian
            this.DialogResult = true; // Đóng popup với kết quả "Có"
        }
    }
}
