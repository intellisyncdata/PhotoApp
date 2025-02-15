using DemoPhotoBooth.DataContext;
using DemoPhotoBooth.Models;
using Microsoft.EntityFrameworkCore;
using SharpVectors.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace DemoPhotoBooth.Pages.Preview
{
    /// <summary>
    /// Interaction logic for PreviewPartial.xaml
    /// </summary>
    public partial class PreviewPartial : Page, INotifyPropertyChanged
    {
        private CommonDbDataContext _db;

        public event PropertyChangedEventHandler PropertyChanged;

        public PreviewPartial()
        {
            InitializeComponent();
            _db = new CommonDbDataContext();
            DataContext = this;
        }

        private bool isPrintPage = false;
        public PreviewPartial(bool isPrintPage)
        {
            InitializeComponent();
            _db = new CommonDbDataContext();
            DataContext = this;
            this.isPrintPage = isPrintPage;
        }

        private bool _isColorBackground;
        private bool IsColorBackground
        {
            get => _isColorBackground;
            set 
            { 
                _isColorBackground = value;
                OnPropertyChanged(); 
            }
        }

        private bool _isImageBackground;
        private bool IsImageBackground
        {
            get => _isImageBackground;
            set 
            { 
                _isImageBackground = value;
                OnPropertyChanged(); 
            }
        }

        private string _currentColor;
        private string CurrentColor
        {
            get => _currentColor;
            set
            {
                _currentColor = value;
                OnPropertyChanged();
            }
        }

        private decimal _height;
        private decimal ActualHeight
        {
            get => _height;
            set { _height = value; OnPropertyChanged(); }
        }

        private decimal _width;
        private decimal ActualWidth
        {
            get => _width;
            set { _width = value; OnPropertyChanged(); }
        }

        private Background _currentBackground;
        public Background CurrentBackground
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

        private Layout _layout;
        public Layout Layout
        {
            get => _layout;
            set
            {
                if (_layout != value)
                {
                    _layout = value;
                    OnPropertyChanged();
                }
            }
        }

        // Gọi sự kiện thông báo thay đổi
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void PreviewPartial_Loaded(object sender, RoutedEventArgs e)
        {
            var item = _db.LayoutApp.AsNoTracking().FirstOrDefault(x => x.IsSelected);

            if (item == null) { return; }
            var newBg = new Background();
            var bgName = item.BackgroudImage?.Split('/').Last() ?? string.Empty;
            newBg.Image = new Uri($"{Directory.GetCurrentDirectory()}/layouts/{bgName}").AbsoluteUri;

            var newLayout = new Layout();
            var layoutName = item.LayoutImage.Split('/').Last() ?? string.Empty;
            var layoutPath = this.isPrintPage ? $"{Directory.GetCurrentDirectory()}/layouts/temp/{layoutName}" : $"{Directory.GetCurrentDirectory()}/layouts/{layoutName}";
            newLayout.ImageUrl = new Uri(layoutPath).AbsoluteUri;

            CurrentBackground = newBg;
            Layout = newLayout;
            ActualHeight = item.Height ?? 400;
            ActualWidth = item.Width ?? 290;
            Dispatcher.Invoke(() =>
            {
                IsColorBackground = item.IsBackgroundColor;
                IsImageBackground = !IsColorBackground;
                CurrentColor = "red" ?? string.Empty;
            });
        }

        public void ReAssignLayout(Layout newLayout)
        {
            Layout = newLayout;
        }

        public string GetSvgUri()
        {
            return Layout.ImageUrl;
        }
    }
}
