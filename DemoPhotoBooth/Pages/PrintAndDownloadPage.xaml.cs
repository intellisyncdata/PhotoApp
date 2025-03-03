using DemoPhotoBooth.Common;
using DemoPhotoBooth.DataContext;
using DemoPhotoBooth.Models.DTOs;
using DemoPhotoBooth.Models.Entities;
using DemoPhotoBooth.Pages.Preview;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using SharpVectors.Converters;
using SharpVectors.Renderers.Wpf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net.Http;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static DemoPhotoBooth.Communicate.MessageTypes;

namespace DemoPhotoBooth.Pages
{
    /// <summary>
    /// Interaction logic for PrintAndDownloadPage.xaml
    /// </summary>
    public partial class PrintAndDownloadPage : Page
    {
        private DispatcherTimer dispatcherTimer = new DispatcherTimer();
        private uint timeStep = 30; // 30s
        private static string imageFolder = string.Empty;
        private string printName = string.Empty;
        private string printFolderPath = $"{imageFolder}/prints";
        private string videoFolderPath = $"{imageFolder}/video";
        private readonly CommonDbDataContext _db;
        private bool isLandscape = false;
        private bool isCutted = false;
        private SerialPort? serialPort;

        public PrintAndDownloadPage()
        {
            InitializeComponent();
            _db = new CommonDbDataContext();
            var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backgrounds/bgprint.png");
            this.BgCustome.ImageSource = new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute));
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();

            LoadImageFolder();
            LoadQR();
            var layout = _db.LayoutApp.AsNoTracking().SingleOrDefault(x => x.IsSelected);

            PrintHandler();
        }

        private void LoadImageFolder()
        {
            var layoutInfo = _db.LayoutApp.AsNoTracking().FirstOrDefault(x => x.IsSelected);
            if (layoutInfo != null)
            {
                imageFolder = layoutInfo.ImageFolderPath ?? string.Empty;
                printFolderPath = $"{imageFolder}/prints";
                videoFolderPath = $"{imageFolder}/video";
                printName = layoutInfo.PrintName ?? "DS-RX1";
                var svg = _db.SvgInfors.FirstOrDefault(x => x.Name == layoutInfo.SVGMappingName);
                if (svg != null)
                {
                    isLandscape = svg.IsLandscape;
                    isCutted = svg.IsCutted;
                }

                if (!string.IsNullOrEmpty(imageFolder))
                {
                    var bitmap = new BitmapImage();
                    using (var stream = File.OpenRead($"{printFolderPath}/image.png"))
                    {
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = stream;
                        bitmap.EndInit();
                    }
                    //finalImage.Source = bitmap;
                    //finalImage.Stretch = Stretch.Uniform;
                    //finalImage.HorizontalAlignment = HorizontalAlignment.Center;
                    //finalImage.VerticalAlignment = VerticalAlignment.Center;
                }
            }
        }

        private void LoadQR()
        {
            var layout = _db.LayoutApp.AsNoTracking().SingleOrDefault(x => x.IsSelected);
            if (layout == null) return;

            var bitmap = Helper.GenerateQRCode(layout.QRLink);
            qrCode.Source = bitmap;
        }

        private async void PrintHandler()
        {
            try
            {
                PrintDialog dlg = new System.Windows.Controls.PrintDialog();
                //dlg.PrintQueue = new System.Printing.PrintQueue(new System.Printing.PrintServer(), isCutted ? "DS-RX1 Cut Feature" : "DS-RX1");
                dlg.PrintQueue = new System.Printing.PrintQueue(new System.Printing.PrintServer(), printName != "DS-RX1" ? "Microsoft Print to PDF" : isCutted ? "DS-RX1 Cut Feature" : "DS-RX1");
                dlg.PrintTicket.CopyCount = 1;
                dlg.PrintTicket.PageOrientation = isCutted ? PageOrientation.Portrait : isLandscape ? PageOrientation.Landscape : PageOrientation.Portrait;
                var layout = _db.LayoutApp.AsNoTracking().SingleOrDefault(x => x.IsSelected);
                DrawingVisual visualImage = new DrawingVisual();
                using (DrawingContext dc = visualImage.RenderOpen())
                {
                    BitmapImage bitmap = new BitmapImage();
                    using (var stream = File.OpenRead($"{layout!.ImageFolderPath}/prints/image.png"))
                    {
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = stream;
                        bitmap.EndInit();
                    }

                    Rect rect = default;
                    double width = default;
                    double height = default;
                    Grid grid = null;
                    Canvas canvas = null;
                    SvgInfor svg = null;
                    double x = 0, y = 0;
                    switch (layout.SVGMappingName)
                    {
                        case "1x4v.svg":
                            //dlg.PrintTicket.PageMediaSize = new PageMediaSize(6.15 * 2.54 * 96, 8.12 * 2.54 * 96);
                            dlg.PrintTicket.PageOrientation = PageOrientation.Portrait;
                            width = dlg.PrintableAreaWidth;
                            height = dlg.PrintableAreaHeight;
                            grid = new Grid();
                            grid.Width = width;
                            grid.Height = height;

                            canvas = new Canvas();
                            svg = _db.SvgInfors.FirstOrDefault(x => x.Name == "1x4v.svg")!;

                            for (int i = 0; i < 2; i++)
                            {
                                var image = new System.Windows.Controls.Image();
                                image.Source = bitmap;

                                image.HorizontalAlignment = HorizontalAlignment.Left;
                                image.VerticalAlignment = VerticalAlignment.Top;
                                Canvas.SetLeft(image, x);
                                Canvas.SetTop(image, y);
                                canvas.Children.Add(image);
                                x += svg!.ActualWidth;
                            }
                            grid.Children.Add(canvas);
                            dc.DrawRectangle(new VisualBrush(grid), null, new Rect(0, 0, width, height));
                            break;
                        case "3x1v.svg":
                            //dlg.PrintTicket.PageMediaSize = new PageMediaSize(6.15 * 2.54 * 96, 8.12 * 2.54 * 96);
                            dlg.PrintTicket.PageOrientation = PageOrientation.Landscape;
                            width = dlg.PrintableAreaWidth;
                            height = dlg.PrintableAreaHeight;

                            grid = new Grid();
                            grid.Width = width;
                            grid.Height = height;

                            canvas = new Canvas();
                            svg = _db.SvgInfors.FirstOrDefault(x => x.Name == "3x1v.svg")!;

                            for (int i = 0; i < 2; i++)
                            {
                                var image = new System.Windows.Controls.Image();
                                image.Source = bitmap;

                                image.HorizontalAlignment = HorizontalAlignment.Left;
                                image.VerticalAlignment = VerticalAlignment.Top;
                                Canvas.SetLeft(image, x);
                                Canvas.SetTop(image, y);
                                canvas.Children.Add(image);
                                y += svg!.ActualHeight;
                            }
                            grid.Children.Add(canvas);
                            dc.DrawRectangle(new VisualBrush(grid), null, new Rect(0, 0, width, height));
                            break;
                        default:
                            width = dlg.PrintableAreaWidth;
                            height = dlg.PrintableAreaHeight;
                            rect = new Rect(0, 0, width, height);
                            dc.DrawImage(bitmap, rect);
                            break;
                    }
                }

                while (layout.PrintQuantity > 0)
                {
                    dlg.PrintVisual(visualImage, "Docs");
                    int number = isCutted == true ? 2 : 1; 
                    layout.PrintQuantity -= number;

                    await Task.Delay(2000);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private async void ClearResource()
        {
            var itemLayout = _db.LayoutApp.AsNoTracking().FirstOrDefault(x => x.IsSelected);
            if ((itemLayout == null)) return;

            try
            {
                var info = _db.LayoutApp.AsNoTracking()?.FirstOrDefault();
                if (info != null)
                {
                  
                    var photoApp = _db.PhotoApps.AsNoTracking().FirstOrDefault();
                    if (photoApp != null)
                    {
                        int retry = 0;
                        while (retry < 3)
                        {
                            var res = await SendNotificationAsync(photoApp!.Id, info.PrintQuantity, photoApp!.Token);
                            if (res) break;
                            retry++;
                            await Task.Delay(3000);
                        }
                    }
                }

                _db.LayoutApp.Remove(itemLayout);
                await _db.SaveChangesAsync();

                if (Directory.Exists(imageFolder))
                {
                    try
                    {
                        System.GC.Collect();
                        System.GC.WaitForPendingFinalizers();
                        var files = Directory.GetFiles(imageFolder);
                        foreach (var file in files)
                        {
                            File.Delete(file);
                        }

                        var folders = Directory.GetDirectories(imageFolder);
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
            catch (Exception ex)
            {

            }
        }

        private async Task<bool> SendNotificationAsync(uint id, int count, string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return false;
            }

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string apiUrl = string.Format(ApiUrl.ApiSendMessage, id);

                    var data = new PingPayload
                    {
                        PrinterCount = count
                    };
                    var json = System.Text.Json.JsonSerializer.Serialize(data);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(token);
                    HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                    return response.IsSuccessStatusCode;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }



        private Visual? DrawVisual()
        {
            var layout = _db.LayoutApp.AsNoTracking().FirstOrDefault(x => x.IsSelected);

            if (layout != null && !string.IsNullOrEmpty(layout?.BackgroudImage))
            {
                var svgInfo = _db.SvgInfors.AsQueryable().AsNoTracking()
                    .FirstOrDefault(x => x.Name == layout.SVGMappingName);

                var svgPath = $"{Directory.GetCurrentDirectory()}/layouts/temp/{layout.SVGMappingName}";

                WpfDrawingSettings settings = new WpfDrawingSettings();
                FileSvgReader svgReader = new FileSvgReader(settings);
                DrawingGroup drawing = svgReader.Read(svgPath);

                BitmapImage bitmapBg = new BitmapImage(new Uri(layout.BackgroudImage));
                DrawingVisual visual = new DrawingVisual();
                using (DrawingContext dc = visual.RenderOpen())
                {
                    // Vẽ hình ảnh nền
                    dc.DrawImage(bitmapBg, new Rect(0, 0, (int)svgInfo!.PrintWidth, (int)svgInfo!.PrintHeight));
                    dc.DrawDrawing(drawing);
                }

                SaveVisualAsHighDpiImage(visual, $"{Directory.GetCurrentDirectory()}/20.01.2025/prints/image.png");

                return visual;
            }

            return null;
        }
        public BitmapSource ResizeBitmap(BitmapSource source, int newWidth, int newHeight)
        {
            TransformedBitmap transformedBitmap = new TransformedBitmap(source,
                new ScaleTransform(newWidth / source.Width, newHeight / source.Height));
            return transformedBitmap;
        }

        private void DispatcherTimer_Tick(object? sender, EventArgs e)
        {
            if (timeStep > 0)
            {
                lblTimer.Text = $"{timeStep--}";
                return;
            }
            dispatcherTimer.Stop();
            NextHomePage();
        }

        private void NextHomePage()
        {
            ClearResource();
            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Close();
                serialPort.Dispose();
            }
            var window = System.Windows.Application.Current.MainWindow as MainWindow;
            if (window != null)
            {
                
                window.MainFrame.Navigate(new HomePage());
            }
        }

        private void SaveVisualAsHighDpiImage(Visual visual, string filePath, int dpi = 600)
        {
            double scale = dpi / 96.0; // Tính tỉ lệ theo DPI mong muốn

            // Lấy kích thước thực tế của visual
            Rect bounds = VisualTreeHelper.GetDescendantBounds(visual);
            int width = (int)(bounds.Width * scale);
            int height = (int)(bounds.Height * scale);

            // Tạo RenderTargetBitmap với kích thước mới
            RenderTargetBitmap rtb = new RenderTargetBitmap(width, height, dpi, dpi, PixelFormats.Pbgra32);
            DrawingVisual dv = new DrawingVisual();

            using (DrawingContext ctx = dv.RenderOpen())
            {
                ctx.DrawRectangle(new VisualBrush(visual), null, bounds);
            }

            rtb.Render(dv);

            // Lưu ảnh với chất lượng cao
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));

            using (FileStream file = File.OpenWrite(filePath))
            {
                encoder.Save(file);
            }
        }

        private void btnEnd_Click(object sender, RoutedEventArgs e)
        {
            dispatcherTimer.Stop();
            //finalImage.Source = null;
            NextHomePage();
        }
    }
}
