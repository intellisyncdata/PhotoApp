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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using DemoPhotoBooth.DataContext;
using DemoPhotoBooth.Models;
using DemoPhotoBooth.Pages.BackgroundPages;
using DemoPhotoBooth.Pages.Preview;

namespace DemoPhotoBooth.Pages
{
    /// <summary>
    /// Interaction logic for CameraMode.xaml
    /// </summary>
    public partial class CameraMode : Page
    {
        private DispatcherTimer _countdownTimer;
        private int _timeRemaining = 30; // 30 giây
        private bool isTimerModeSelected = false;
        private string selectedMode = "";
        private readonly CommonDbDataContext _db;
        private Layout _layout { get; set; }
        private List<Layout> _listLayouts { get; set; }

        public CameraMode(Layout layout, List<Layout> listLayouts)
        {
            InitializeComponent();
            var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backgrounds/bgcameramode.png");
            this.BgCustome.ImageSource = new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute));
            _layout = layout;
            _listLayouts = listLayouts;
            selectedMode = "Timer";
            _db = new CommonDbDataContext();
            _countdownTimer = new DispatcherTimer();
            _countdownTimer.Interval = TimeSpan.FromSeconds(1); // Mỗi giây chạy một lần
            _countdownTimer.Tick += CountdownTimer_Tick;
            _countdownTimer.Start();
        }

        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            if (_timeRemaining > 0)
            {
                _timeRemaining--;
                txtCountdown.Text = $"{_timeRemaining}";
            }
            else
            {
                _countdownTimer.Stop();
                var item = _db.LayoutApp.FirstOrDefault();
                if (item != null && item.FrameType == "vertical")
                {
                    NavigationService?.Navigate(new CameraPage(_layout, _listLayouts, true));
                }
                else if (item != null && item.FrameType == "horizontal")
                {
                    NavigationService?.Navigate(new CameraPage(_layout, _listLayouts, false));
                }
            }
        }

        private void ManualMode_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var item = _db.LayoutApp.FirstOrDefault();
            if (item != null && item.FrameType == "vertical")
            {
                NavigationService?.Navigate(new ManualCameraPage(_layout, _listLayouts, true));
            }
            else
            {
                NavigationService?.Navigate(new ManualCameraPage(_layout, _listLayouts, false));
            }
        }

        private void TimerMode_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var item = _db.LayoutApp.FirstOrDefault();
            if (item != null && item.FrameType == "vertical")
            {
                NavigationService?.Navigate(new CameraPage(_layout, _listLayouts, true));
            }
            else
            {
                NavigationService?.Navigate(new CameraPage(_layout, _listLayouts, false));
            }
        }

        private void Continue_Click(object sender, RoutedEventArgs e)
        {
            if (selectedMode == "Timer")
            {
                //NavigationService?.Navigate(new NewPreviewPage(_layout, _listLayouts));
                TimerMode_Click(sender, e); // Gọi hàm Hẹn Giờ nếu chọn Timer
            }
            else if (selectedMode == "Manual")
            {
                ManualMode_Click(sender, e); // Gọi hàm Điều Khiển nếu chọn Manual
            }
            else
            {
                MessageBox.Show("Vui lòng chọn chế độ chụp trước khi tiếp tục!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void StackPanel_Click(object sender, MouseButtonEventArgs e)
        {
            // Đặt màu viền tất cả về Transparent trước
            TimerBorder.Opacity = 0.5;
            ManualBorder.Opacity = 0.5;

            // Xác định StackPanel nào được nhấn
            if (sender is StackPanel stackPanel)
            {
                if (stackPanel.Name == "TimerStackPanel")
                {
                    selectedMode = "Timer";
                    TimerBorder.Opacity = 1;
                }
                else if (stackPanel.Name == "ManualStackPanel")
                {
                    selectedMode = "Manual";
                    ManualBorder.Opacity = 1;
                }
            }
        }

    }
}
