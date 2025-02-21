using DemoPhotoBooth.DataContext;
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

        public NewPreviewPage(bool portraitMode = false)
        {
            InitializeComponent();
            DataContext = this;
            isPotrait = portraitMode;
            var path = "pack://application:,,,/Layouts/bg-preview-horizontal.png";
            DockPanelListImage.Margin = new Thickness(0, 0, 0, 0);
            if (portraitMode)
            {
                path = "pack://application:,,,/Layouts/bg-preview-vertical.png";
                DockPanelListImage.Margin = new Thickness(100, 0, 0, 0);
            }
            this.BackgroundPreview.ImageSource = new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute));
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            App.EventAggregator.GetEvent<PreviewPageNextPage>().Subscribe(context =>
            {
                NavigationService.Navigate(new PrintAndDownloadPage());
            });

            imagesSelected.Navigate(new ListImagesPage(isPotrait));
            layoutPreview.Navigate(new PreviewLayoutPartial(isPotrait));
        }
    }
}
