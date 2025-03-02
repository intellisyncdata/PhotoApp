using DemoPhotoBooth.DataContext;
using DemoPhotoBooth.Models;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static DemoPhotoBooth.Communicate.MessageTypes;

namespace DemoPhotoBooth.Pages.Preview
{
    /// <summary>
    /// Interaction logic for NewPreviewPage.xaml
    /// </summary>
    public partial class NewPreviewPage : Page
    {
        bool isPotrait = false;
        private Layout _layout { get; set; }
        private List<Layout> _listLayouts { get; set; }

        public NewPreviewPage(Layout layout, List<Layout> listLayouts, bool portraitMode = false)
        {
            InitializeComponent();
            _layout = layout;
            _listLayouts = listLayouts;
            DataContext = this;
            isPotrait = portraitMode;
            
            var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backgrounds/bgpreviewhorizontal.png");
            DockPanelListImage.Margin = new Thickness(0, 0, 0, 0);
            if (portraitMode)
            {
                path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backgrounds/bgpreviewvertical.png");
                DockPanelListImage.Margin = new Thickness(100, 0, 0, 0);
            }
            this.BackgroundPreview.ImageSource = new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute));
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            imagesSelected.Navigate(new ListImagesPage(isPotrait));
            layoutPreview.Navigate(new PreviewLayoutPartial(_layout, _listLayouts, isPotrait));
        }
    }
}
