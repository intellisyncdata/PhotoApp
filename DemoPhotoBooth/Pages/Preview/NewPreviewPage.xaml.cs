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

        public NewPreviewPage()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            App.EventAggregator.GetEvent<PreviewPageNextPage>().Subscribe(context =>
            {
                NavigationService.Navigate(new PrintAndDownloadPage());
            });

            imagesSelected.Navigate(new ListImagesPage());
            layoutPreview.Navigate(new PreviewLayoutPartial());
        }
    }
}
