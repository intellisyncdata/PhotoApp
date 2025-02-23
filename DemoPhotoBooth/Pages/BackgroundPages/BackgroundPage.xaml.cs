using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation; // Thêm namespace cho hiệu ứng
using System.Windows.Threading;
using DemoPhotoBooth.DataContext;
using DemoPhotoBooth.Models;
using DemoPhotoBooth.Models.DTOs;
using DemoPhotoBooth.Pages.Preview;
using SharpVectors.Converters;
using SharpVectors.Renderers.Wpf;

namespace DemoPhotoBooth.Pages.BackgroundPages
{
    public class ThemeOption
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public partial class BackgroundPage : Page, INotifyPropertyChanged
    {
        public PhotoApp PhotoApp { get; set; }
        public List<BgLayout> Backgrounds { get; set; }
        public Layout Layout { get; set; }
        public List<Layout> ListLayout { get; set; }
        private int _currentIndex;
        private Layout _selectedLayout;
        public event PropertyChangedEventHandler PropertyChanged;

        private List<string> _colors;
        private string color;
        private int _currentColorIndex;
        private readonly CommonDbDataContext _db;
        private int _currentPage = 0;
        private const int ItemsPerPage = 3;
        private DispatcherTimer countdownTimer;
        private bool isPopupShown = false;
        private bool _isPay = false;
        public ObservableCollection<BgLayout> VisibleBackgrounds { get; set; } = new ObservableCollection<BgLayout>();

        public BackgroundPage(Layout layout, string colors, List<Layout> listLayout, bool isPay = false)
        {
            InitializeComponent();
            ListLayout = listLayout;
            color = colors;
            Layout = layout;
            _db = new CommonDbDataContext();
            // Load dữ liệu ban đầu
            _currentIndex = 0;
            _colors = new List<string>(colors.Split(','));
            _currentColorIndex = 0;
            _selectedLayout = layout;
            _isPay = isPay;
            btnPrevious.IsEnabled = true;
            btnPrevious.Opacity = 1;
            if (isPay)
            {
                btnPrevious.IsEnabled = false;
                btnPrevious.Opacity = 0.5;
            }

            // List ra Themes theo Layout chỉ định
            var themes = layout.themes
                .Select(t => new ThemeOption
                {
                    Id = t.id,
                    Name = t.theme_type.name
                })
                .ToList();

            topicsListBox.ItemsSource = layout.themes;

            // Thiết lập trạng thái ban đầu
            IsImageGridVisible = true;
            IsColorGridVisible = false;
            DataContext = this;
            btnContinue.IsEnabled = false;
            btnContinue.Opacity = 0.5;
            //UpdateVisibleBackgrounds();
            if (layout.themes.Count > 0)
            {
                Backgrounds = layout.themes[0].bg_layouts;
                UpdateVisibleBackgrounds();
            }

            // Hiệu ứng mượt mà khi khởi tạo
            Loaded += OnPageLoaded;
        }
        private ScrollViewer _scrollViewer;
        private bool _isMouseDown = false;
        private Point _mouseStartPosition;
        private double _scrollStartOffset;

        private void ListBox_Loaded(object sender, RoutedEventArgs e)
        {
            _scrollViewer = GetScrollViewer(topicsListBox);
        }

        private void ListBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_scrollViewer != null)
            {
                _isMouseDown = true;
                _mouseStartPosition = e.GetPosition(topicsListBox);
                _scrollStartOffset = _scrollViewer.HorizontalOffset;
                topicsListBox.CaptureMouse();
            }
        }

        private void ListBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isMouseDown && _scrollViewer != null)
            {
                Point currentPosition = e.GetPosition(topicsListBox);
                double deltaX = _mouseStartPosition.X - currentPosition.X;
                _scrollViewer.ScrollToHorizontalOffset(_scrollStartOffset + deltaX);
            }
        }

        private void ListBox_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _isMouseDown = false;
            topicsListBox.ReleaseMouseCapture();
        }

        private ScrollViewer GetScrollViewer(DependencyObject depObj)
        {
            if (depObj is ScrollViewer)
                return (ScrollViewer)depObj;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);
                var result = GetScrollViewer(child);
                if (result != null)
                    return result;
            }
            return null;
        }
        private async void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            // Đặt màu nền ban đầu
            var initialColor = (Color)ColorConverter.ConvertFromString("#CEE5EB");
            Background = new SolidColorBrush(initialColor);

            // Tạo Storyboard để thực hiện Fade-in
            var storyboard = new Storyboard();

            // Hiệu ứng Fade-in (Opacity)
            var fadeIn = new DoubleAnimation
            {
                From = 0, // Bắt đầu mờ
                To = 1,   // Hiện rõ hoàn toàn
                Duration = TimeSpan.FromMilliseconds(100)
            };
            Storyboard.SetTargetProperty(fadeIn, new PropertyPath("Opacity"));

            // Thêm vào Storyboard và bắt đầu
            storyboard.Children.Add(fadeIn);
            storyboard.Begin(this);

            // Chờ hiệu ứng hoàn tất
            await Task.Delay(150);

            // Cập nhật dữ liệu
            UpdateCurrentImage();
        }

        private BgLayout chooseBackground;
        private BgLayout _currentBackground;
        public BgLayout CurrentBackground
        {
            get => _currentBackground;
            set
            {
                if (_currentBackground != value)
                {
                    _currentBackground = value;
                    OnPropertyChanged();
                }
            }
        }

        private Color _currentColor;
        public Color CurrentColor
        {
            get => _currentColor;
            set
            {
                if (_currentColor != value)
                {
                    _currentColor = value;
                    OnPropertyChanged();
                }
            }
        }

        private Color oldColor = new Color();

        private bool _isImageGridVisible = true;
        public bool IsImageGridVisible
        {
            get => _isImageGridVisible;
            set
            {
                if (_isImageGridVisible != value)
                {
                    _isImageGridVisible = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isColorGridVisible = false;
        public bool IsColorGridVisible
        {
            get => _isColorGridVisible;
            set
            {
                if (_isColorGridVisible != value)
                {
                    _isColorGridVisible = value;
                    OnPropertyChanged();
                }
            }
        }

        // Gọi sự kiện thông báo thay đổi
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void UpdateCurrentImage()
        {
            if (Backgrounds != null && Backgrounds.Count > 0)
            {
                CurrentBackground = Backgrounds[_currentIndex];
            }
        }



        #region Background Logic
        public void OnBackgroundClicked(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is BgLayout clickedBackground)
            {
                foreach (var bg in Backgrounds)
                {
                    bg.IsSelected = false;
                }
                clickedBackground.IsSelected = true;
                chooseBackground = clickedBackground;
                OnPropertyChanged(nameof(VisibleBackgrounds));
                btnContinue.IsEnabled = true;
                btnContinue.Opacity = 1;
            }
        }

        private void UpdateVisibleBackgrounds()
        {
            VisibleBackgrounds.Clear();
            int startIndex = _currentPage * ItemsPerPage;
            for (int i = 0; i < ItemsPerPage && startIndex + i < Backgrounds.Count; i++)
            {
                VisibleBackgrounds.Add(Backgrounds[startIndex + i]);
            }
            OnPropertyChanged(nameof(VisibleBackgrounds));
        }

        public void PreviousBackground_Click(object sender, RoutedEventArgs e)
        {
            if (Backgrounds != null && Backgrounds.Count > 0)
            {
                _currentPage = (_currentPage > 0) ? _currentPage - 1 : (Backgrounds.Count - 1) / ItemsPerPage;
                UpdateVisibleBackgrounds();
            }
        }

        public void NextBackground_Click(object sender, RoutedEventArgs e)
        {
            if (Backgrounds != null && Backgrounds.Count > 0)
            {
                _currentPage = (_currentPage < (Backgrounds.Count - 1) / ItemsPerPage) ? _currentPage + 1 : 0;
                UpdateVisibleBackgrounds();
            }
        }

        #endregion

        #region Color Logic 
        private void UpdateCurrentColor()
        {
            //if (_colors != null && _colors.Count > 0)
            //{
            //    CurrentColor = (Color)ColorConverter.ConvertFromString(_colors[_currentColorIndex]);
            //    if (MySvgCanvas.Drawings != null)
            //    {
            //        ChangeSvgFill(MySvgCanvas.Drawings, CurrentColor, oldColor);
            //        oldColor = CurrentColor;
            //    }
            //}
        }

        private string ColorToHex(Color color)
        {
            return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
        }


        private void NextColorButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentColorIndex < _colors.Count - 1)
            {
                _currentColorIndex++;
                UpdateCurrentColor();
            }
        }

        private void MySvgCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            //if (MySvgCanvas.Drawings != null)
            //{
            //    ChangeSvgFill(MySvgCanvas.Drawings, CurrentColor, oldColor); // Thay đổi màu fill thành đỏ
            //}
        }

        private void ChangeSvgFill(Drawing drawing, Color newColor, Color oldColor)
        {
            if (drawing is GeometryDrawing geoDrawing)
            {
                // Kiểm tra xem màu hiện tại có phải #ababab không
                if (geoDrawing.Brush is SolidColorBrush solidBrush)
                {
                    if (solidBrush.Color == Color.FromArgb(255, 171, 171, 171))
                    {
                        geoDrawing.Brush = new SolidColorBrush(newColor);
                    }
                    else if (solidBrush.Color == oldColor)
                    {
                        geoDrawing.Brush = new SolidColorBrush(newColor);
                    }
                }
            }
            else if (drawing is DrawingGroup drawingGroup)
            {
                foreach (Drawing childDrawing in drawingGroup.Children)
                {
                    ChangeSvgFill(childDrawing, newColor, oldColor); // Đệ quy duyệt qua tất cả các phần tử con
                }
            }
        }

        private void PreviousColorButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentColorIndex > 0)
            {
                _currentColorIndex--;
                UpdateCurrentColor();
            }
        }
        #endregion

        #region Show-Hide Grid
        private void ShowColorGrid_Click(object sender, RoutedEventArgs e)
        {
            // Hiệu ứng chuyển đổi giữa các grid
            ApplyTransitionEffect(() =>
            {
                IsImageGridVisible = false;
                IsColorGridVisible = true;
            });
            UpdateCurrentColor();
        }

        private void ShowImageGrid_Click(object sender, RoutedEventArgs e)
        {
            // Hiệu ứng chuyển đổi giữa các grid
            ApplyTransitionEffect(() =>
            {
                IsImageGridVisible = true;
                IsColorGridVisible = false;
            });
        }

        private void ThemeButton_Click(object sender, RoutedEventArgs e)
        {
            // Lấy button được click
            Button clickedButton = sender as Button;

            if (clickedButton != null)
            {
                // Lấy DataContext của button (chính là item của ListBox)
                Theme selectedTheme = clickedButton.DataContext as Theme; // Ép kiểu thành kiểu dữ liệu của bạn

                if (selectedTheme != null)
                {
                    // Hiển thị thông tin item được chọn
                    _currentPage = 0;
                    Backgrounds = selectedTheme.bg_layouts;
                    UpdateVisibleBackgrounds();
                    //MessageBox.Show($"Bạn đã chọn: {selectedTheme.theme_type.name}");
                }
            }
        }

        private void ApplyTransitionEffect(Action transitionAction)
        {
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300));
            fadeOut.Completed += (s, e) =>
            {
                transitionAction();
                var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300));
                BeginAnimation(OpacityProperty, fadeIn);
            };
            BeginAnimation(OpacityProperty, fadeOut);
        }
        #endregion

        #region Navigation 
        private void NavigateHome_Click(object sender, RoutedEventArgs e)
        {
            foreach (var bg in Backgrounds)
            {
                bg.IsSelected = false;
            }

            chooseBackground = null;

            OnPropertyChanged(nameof(VisibleBackgrounds));

            NavigationService?.Navigate(new LayoutPage(ListLayout));
        }


        private void NavigatePayment_Click(object sender, RoutedEventArgs e)
        {
            var layoutApp = new LayoutApp();
            layoutApp.LayoutImage = Layout.ImageUrl;
            if (IsImageGridVisible)
            {
                if (chooseBackground != null)
                {
                    layoutApp.BackgroudImage = chooseBackground.image_url;
                }
                else
                {
                    if (!isPopupShown)
                    {
                        // Hiển thị popup khi hết thời gian
                        ShowPopup();
                        isPopupShown = true;
                        return;
                    }
                }

            }
            else
            {
                layoutApp.Color = ColorToHex(CurrentColor);
            }

            layoutApp.FrameType = Layout.FrameType;
            layoutApp.Width = Layout.Width;
            layoutApp.Height = Layout.Height;

            SaveLayoutApp(layoutApp);
            if (_isPay)
            {
                NavigationService?.Navigate(new NewPreviewPage(Layout, ListLayout));
            }
            else
            {
                NavigationService?.Navigate(new PaymentPage(Layout.Price, 1, Layout, Backgrounds, color, ListLayout));
            }
        }
        #endregion

        private void ShowPopup()
        {
            // Hiển thị popup tùy chỉnh
            Popup popup = new Popup
            {
                Owner = Window.GetWindow(this), // Gắn popup với cửa sổ hiện tại
            };

            bool? result = popup.ShowDialog(); // Hiển thị popup và chờ kết quả

            if (result == true && popup.UserWantsMoreTime)
            {
                // Thêm 30 giây
                isPopupShown = false;
            }
            else
            {
                // Quay về trang chủ
                NavigateToHomePage(this, new RoutedEventArgs());
            }
        }

        private void NavigateToHomePage(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new HomePage());
        }

        private int GetMaximumSelected(string svgName)
        {
            if (string.IsNullOrEmpty(svgName)) return 4;

            switch (svgName)
            {
                case "2x2h.svg": return 4;
                case "1x3v13.svg": return 4;
                case "1x4v.svg": return 4;
                case "2x2v.svg": return 4;
                case "2x4v.svg": return 8;
                case "3x1v.svg": return 3;
                case "2x4v_1.svg": return 8;
                default: return 4;
            }
        }

        private void CheckLayoutApp()
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

        private void SaveLayoutApp(LayoutApp result)
        {
            try
            {
                CheckLayoutApp();
                CheckFolderImageApp();
                var eLayoutApp = new DemoPhotoBooth.Models.Entities.LayoutApp(result);
                eLayoutApp.IsSelected = true;
                eLayoutApp.IsBackgroundColor = IsColorGridVisible;
                var svgName = eLayoutApp.LayoutImage.Split("/")?.Last();

                eLayoutApp.SVGMappingName = $"{svgName?.Split("-")[0]}.svg";
                eLayoutApp.LayoutImage = svgName ?? string.Empty;
                eLayoutApp.Quantity = GetMaximumSelected(eLayoutApp.SVGMappingName);
                _db.LayoutApp.Add(eLayoutApp);

                _db.SaveChanges();

            }
            catch (Exception ex)
            {

            }
        }

        private void CheckFolderImageApp()
        {
            string path = $"{Directory.GetCurrentDirectory()}/{DateTime.Now.ToString("dd.MM.yyyy")}";
            if (Directory.Exists(path))
            {
                try
                {
                    System.GC.Collect();
                    System.GC.WaitForPendingFinalizers();
                    var files = Directory.GetFiles(path);
                    foreach (var file in files)
                    {
                        File.Delete(file);
                    }

                    var folders = Directory.GetDirectories(path);
                    foreach (var item in folders)
                    {
                        files = Directory.GetFiles(item);
                        foreach (var file in files)
                        {
                            File.Delete(file);
                        }
                    }
                }
                catch (Exception e)
                {
                }
            }
        }
    }
}
