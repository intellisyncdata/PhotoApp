using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
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
using DemoPhotoBooth.Common;
using DemoPhotoBooth.Models;
using DemoPhotoBooth.Models.DTOs;
using DemoPhotoBooth.Models.Entities;
using DemoPhotoBooth.Pages.BackgroundPages;

namespace DemoPhotoBooth.Pages
{
    /// <summary>
    /// Interaction logic for LayoutPage.xaml
    /// </summary>
    public partial class LayoutPage : Page
    {
        public List<Layout> Layouts { get; set; }
        public List<Background> Backgrounds { get; set; }

        public LayoutPage(List<Layout> layouts)
        {
            InitializeComponent();
            Layouts = layouts;
            DataContext = this;
        }

        private void OnLayoutClick(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is Layout layout)
            {
                var backgrounds = GetBackgroundsByLayoutId(layout.Id);
                if (backgrounds != null)
                {
                    string bgColorsString = string.Join(", ", layout.BackgroundColor);
                    NavigationService.Navigate(new BackgroundPage(layout, backgrounds, bgColorsString, Layouts));
                }
                else
                {

                }
            }
        }

        private void NavigateHome_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Điều hướng về trang Home
            NavigationService?.Navigate(new HomePage());
        }


        private static string ConvertToPackUri(string relativePath)
        {
            // Xóa dấu `/` ở đầu nếu có
            if (relativePath.StartsWith("/"))
            {
                relativePath = relativePath.TrimStart('/');
            }

            // Trả về Pack URI
            return $"pack://application:,,,/{relativePath}";
        }

        private List<Background> GetBackgroundsByLayoutId(int layoutId)
        {
            List<Background> backgrounds = new List<Background>();

            var selectedLayout = Layouts.FirstOrDefault(l => l.Id == layoutId);

            if (selectedLayout != null)
            {
                backgrounds.AddRange(selectedLayout.BgLayouts); 
            }

            return backgrounds;
        }

    }
}

