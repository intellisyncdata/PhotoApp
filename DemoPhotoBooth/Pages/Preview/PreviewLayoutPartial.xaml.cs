﻿using DemoPhotoBooth.Common;
using DemoPhotoBooth.Communicate;
using DemoPhotoBooth.DataContext;
using DemoPhotoBooth.Models;
using DemoPhotoBooth.Models.Entities;
using Microsoft.EntityFrameworkCore;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
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
using System.Windows.Threading;
using static DemoPhotoBooth.Communicate.MessageTypes;
using static DemoPhotoBooth.Pages.Preview.ListImagesPage;
using static System.Net.Mime.MediaTypeNames;
using Image = System.Windows.Controls.Image;
using Rect = System.Windows.Rect;
using System.Text.Json.Serialization;
using System.Text.Json;
using DemoPhotoBooth.Models.DTOs;
using System.IO.Ports;
using DemoPhotoBooth.Pages.BackgroundPages;

namespace DemoPhotoBooth.Pages.Preview
{
    /// <summary>
    /// Interaction logic for PreviewLayoutPartial.xaml
    /// </summary>
    public partial class PreviewLayoutPartial : Page, INotifyPropertyChanged
    {
        private CommonDbDataContext _db;
        private Dictionary<uint, Image> dicImages = new Dictionary<uint, Image>();
        private double _imageBgWidth;
        private double _imageBgHeight;
        private Canvas _canvas;
        private DispatcherTimer dispatcherTimer = new DispatcherTimer();
        private uint timeStep = 90; // 90s
        private bool isPopupShown = false;
        bool isPortrait = false;
        private int quantity = 0;
        public event PropertyChangedEventHandler? PropertyChanged;
        private Layout _layout { get; set; }
        private List<Layout> _listLayouts { get; set; }

        public double ImageBgWidth
        {
            get => _imageBgWidth;
            set
            {
                _imageBgWidth = value;
                OnPropertyChanged();
            }
        }
        public double ImageBgHeight
        {
            get => _imageBgHeight;
            set
            {
                _imageBgHeight = value;
                OnPropertyChanged();
            }
        }
        public PreviewLayoutPartial(Layout layout, List<Layout> listLayouts, bool portraitMode = false)
        {
            InitializeComponent();
            _db = new CommonDbDataContext();
            DataContext = this;
            isPortrait = portraitMode;
            _layout = layout;
            _listLayouts = listLayouts;
            //btnReset.Visibility = Visibility.Hidden;
            //if (isPortrait)
            //{
            //    Grid.SetRow(gridImages, 1);
            //    Grid.SetColumn(gridImages, 0);
            //    Grid.SetRowSpan(gridImages, 3);
            //    Grid.SetColumnSpan(gridImages, 6);
            //}
            //else
            //{
            //    Grid.SetRow(gridImages, 1);
            //    Grid.SetColumn(gridImages, 0);
            //    Grid.SetRowSpan(gridImages, 4);
            //    Grid.SetColumnSpan(gridImages, 5);
            //}
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();

            btnPrint.IsEnabled = false;
            var layoutSelected = _db.LayoutApp
                    .AsNoTracking()
                    .SingleOrDefault(x => x.IsSelected);

            if (layoutSelected != null)
            {
                RegisterReceiver();
                var svg = _db.SvgInfors.Include(x => x.SvgRectTags)
                    .SingleOrDefault(x => x.Name == layoutSelected.SVGMappingName);
                var svgDetail = svg?.SvgRectTags.ToList() ?? new List<SvgRectTag>();

                var bitmap = new BitmapImage(new Uri($"{layoutSelected.BackgroudImage}"));

                imageBg.Source = bitmap;
                ImageBgWidth = svg!.ActualWidth;
                ImageBgHeight = svg!.ActualHeight;

                DrawImageControl(svgDetail, svg!.ActualWidth, svg!.ActualHeight);
                quantity = layoutSelected.Quantity ?? 0;
            }
        }

        private void DispatcherTimer_Tick(object? sender, EventArgs e)
        {
            if (timeStep > 0)
            {
                lblTimer.Text = $"{timeStep--}s";
                return;
            }
            else if (!isPopupShown)
            {
                // Hiển thị popup khi hết thời gian
                ShowPreviewPopup();
                isPopupShown = true;
            }
        }

        private void NextDownloadPage()
        {
            dispatcherTimer.Stop();
            dispatcherTimer.Tick -= DispatcherTimer_Tick;

            var window = System.Windows.Application.Current.MainWindow as MainWindow;
            if (window != null)
            {
                _db?.Dispose(); _db?.Dispose();
                window.MainFrame.Navigate(new PrintAndDownloadPage());
            }

            CleanupMemory();
        }


        private void ShowPreviewPopup()
        {
            dispatcherTimer.Stop();

            // Hiển thị popup tùy chỉnh
            PreviewPopup popup = new PreviewPopup
            {
                Owner = System.Windows.Window.GetWindow(this), // Gắn popup với cửa sổ hiện tại
            };

            bool? result = popup.ShowDialog(); // Hiển thị popup và chờ kết quả

            if (result == true && popup.UserWantsMoreTime)
            {
                // Thêm 30 giây
                timeStep = 30;
                isPopupShown = false;
                dispatcherTimer.Start();
                lblTimer.Text = $"{timeStep--}s";
            }
            else
            {
                if (popup.userAvailable)
                {
                    btnPrint_Click(btnPrint, new RoutedEventArgs());
                }
                else
                {
                    // Quay về trang chủ
                    NavigateToHomePage(this, new RoutedEventArgs());
                }
            }
        }
       
        private void NavigateToHomePage(object sender, RoutedEventArgs e)
        {
            dispatcherTimer.Stop();  // Dừng Timer để tránh tiếp tục chạy ngầm
            dispatcherTimer.Tick -= DispatcherTimer_Tick; // Hủy đăng ký sự kiện
            _db?.Dispose();

            NavigationService?.Navigate(new HomePage());
            CleanupMemory(); // Giải phóng bộ nhớ
        }

        private void CleanupMemory()
        {
            foreach (var image in dicImages.Values)
            {
                image.MouseDown -= Image_MouseDown;
                image.TouchDown -= Image_TouchDown;
                image.Source = null;
            }
            dicImages.Clear(); // Giải phóng Dictionary
            gridImages.Children.Clear(); // Xóa UI Elements

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        public void Dispose()
        {
            _db?.Dispose();
        }

        private void DrawQR()
        {
            var layout = _db.LayoutApp.AsNoTracking().SingleOrDefault(x => x.IsSelected);
            if (layout != null)
            {
                var svg = _db.SvgInfors
                    .Include(x => x.SvgRectTags).
                    SingleOrDefault(x => x.Name == layout.SVGMappingName);

                if (svg != null)
                {
                    var pnImageQR = dicImages.SingleOrDefault(x => ((dynamic)x.Value.DataContext).isQR);

                    if (pnImageQR.Value != null)
                    {
                        var bmQR = Helper.GenerateQRCode(layout.QRLink);
                        pnImageQR.Value.Source = bmQR;
                    }
                }
            }
        }

        private void RegisterReceiver()
        {
            App.EventAggregator.GetEvent<Page1ToPage2>().Subscribe(context =>
            {
                BitmapImage bitmap = new BitmapImage();
                using (var stream = File.OpenRead(context.rootPath))
                {
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                }
                var pnItem = dicImages.OrderBy(x => x.Key)
                .FirstOrDefault(x => x.Value.Source == null);

                if (pnItem.Value != null)
                {
                    dynamic obj = pnItem.Value.DataContext;
                    var newObj = new
                    {
                        id = obj.id,
                        isQR = obj.isQR,
                        pathImage = (context.uid as Uri).AbsolutePath,
                        name = context.gridName,
                        uri = (context.uid as Uri),
                        rootPath = context.rootPath
                    };
                    pnItem.Value.DataContext = newObj;
                    if (!obj.isQR)
                    {
                        pnItem.Value.Source = bitmap;
                    }

                    var fullAssigned = dicImages.Values
                    .Where(x => !((dynamic)x.DataContext).isQR)
                    .All(x => x.Source != null);

                    if (fullAssigned)
                    {
                        btnPrint.IsEnabled = true;
                    }
                }
            });
        }

        private void DrawImageControl(List<SvgRectTag> svgRects, double width, double height)
        {
            _canvas = new Canvas();
            _canvas.Height = height;
            _canvas.Width = width;
            foreach (var item in svgRects)
            {
                var image = new Image();
                image.Source = null;
                image.DataContext = new
                {
                    isQR = item.IsQRRect,
                    pathImage = string.Empty,
                    id = item.No
                };
                image.Width = item.Width.ToDouble();
                image.Height = item.Height.ToDouble();
                image.Stretch = Stretch.UniformToFill;
                Canvas.SetLeft(image, item.X.ToDouble());
                Canvas.SetTop(image, item.Y.ToDouble());
                image.MouseDown += Image_MouseDown;
                image.TouchDown += Image_TouchDown;
                dicImages.Add(item.No, image);
                _canvas.Children.Add(image);
            }
            gridImages.Children.Add(_canvas);
            if (svgRects.Count < quantity)
            {
                btnPrint.Opacity = 0.5;
                btnPrint.IsEnabled = false;
            }
            else
            {
                btnPrint.Opacity = 1;
                btnPrint.IsEnabled = true;
            }
        }

        private void Image_TouchDown(object? sender, TouchEventArgs e)
        {
            var imageSelected = sender as Image;

            if (imageSelected != null)
            {
                dynamic obj = imageSelected.DataContext;
                dicImages.TryGetValue(obj.id, out Image image);
                if (image != null)
                {
                    imageSelected.Source = null;
                    image.Source = null;
                    App.EventAggregator.GetEvent<Page2ToPage1>().Publish(obj);
                    ShowOrHiddenButton();
                }
            }
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var imageSelected = sender as Image;

            if (imageSelected != null)
            {
                dynamic obj = imageSelected.DataContext;
                dicImages.TryGetValue(obj.id, out Image image);
                if (image != null)
                {
                    imageSelected.Source = null;
                    image.Source = null;
                    App.EventAggregator.GetEvent<Page2ToPage1>().Publish(obj);
                    ShowOrHiddenButton();
                }
            }
        }

        private void ShowOrHiddenButton()
        {
            var allEmpty = dicImages.Values
                .Where(x => !((dynamic)x.DataContext).isQR)
                .All(x => x.Source == null);

            if (allEmpty)
            {
                btnPrint.IsEnabled = false;
            }
            else
            {
                btnPrint.IsEnabled = false;
            }
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            //if (dicImages.Values.Any())
            //{
            //    btnReset.IsEnabled = false;
            //    btnPrint.IsEnabled = false;
            //}
            //dicImages.Values.ToList().ForEach(x =>
            //{
            //    dynamic obj = x.DataContext;
            //    if (!obj.isQR)
            //    {
            //        x.Source = null;
            //        App.EventAggregator.GetEvent<Page2ToPage1>().Publish(x.DataContext);
            //    }
            //});
            var window = System.Windows.Application.Current.MainWindow as MainWindow;
            if (window != null)
            {
                window.MainFrame.Navigate(new BackgroundPage(_layout, "#FFFFFF", _listLayouts, true, 30)); // MainFrame là Frame chính trong MainWindow
            }
            //NavigationService?.Navigate(new BackgroundPage(_layout, "#FFFFFF", _listLayouts));
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            AdsPopup popup = new AdsPopup
            {
                Owner = System.Windows.Window.GetWindow(this), // Gắn popup với cửa sổ hiện tại
            };
            popup.Show();
            var photoApp = await _db.PhotoApps.AsNoTracking().SingleOrDefaultAsync();
            if (photoApp != null)
            {
                var layout = await _db.LayoutApp.AsNoTracking().SingleOrDefaultAsync(x => x.IsSelected);
                if (layout != null)
                {
                    DrawToImage();
                    int retry = 0;
                    btnReset.IsEnabled = btnPrint.IsEnabled = false;
                    while (retry < 3)
                    {
                        var result = await UploadMedia(photoApp, layout!.ImageFolderPath, layout.PaymentId);
                        if (result != null && result.IsSuccessStatusCode)
                        {
                            string json = await result.Content.ReadAsStringAsync();
                            if (!string.IsNullOrEmpty(json))
                            {
                                var resObj = JsonSerializer.Deserialize<MediaUploadResponse>(json);
                                layout.QRLink = resObj?.MediaUrl ?? string.Empty;
                                _db.LayoutApp.Update(layout);
                                await _db.SaveChangesAsync();

                                DrawQR();
                                DrawToImage();
                                break;
                            }
                        }
                        retry++;
                        await Task.Delay(3000);
                    }
                    NextDownloadPage();
                }
            }
        }

        private async Task<HttpResponseMessage?> UploadMedia(Models.Entities.PhotoApp photoApp, string? imageFolder, int? paymentId)
        {
            if (paymentId == null || string.IsNullOrEmpty(imageFolder)) return null;
            using (HttpClient client = new HttpClient())
            {
                // URL API cần gửi dữ liệu
                string url = string.Format(ApiUrl.ApiUploadMedia, photoApp.Id, paymentId);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(photoApp.Token);

                // Tạo form-data content
                using (MultipartFormDataContent formData = new MultipartFormDataContent())
                {
                    // Đọc file và gửi lên server (nếu có)
                    string imgPath = $"{imageFolder}/prints/image.png";
                    string videofilePath = $"{imageFolder}/video/recording.mp4";

                    if (!File.Exists(imgPath) || !File.Exists(videofilePath)) return null;

                    byte[] fileBytes = File.ReadAllBytes(imgPath);
                    ByteArrayContent fileContent = new ByteArrayContent(fileBytes);
                    fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/png");
                    formData.Add(fileContent, "images", System.IO.Path.GetFileName(imgPath));

                    byte[] vdfileBytes = File.ReadAllBytes(videofilePath);
                    ByteArrayContent fileVideoContent = new ByteArrayContent(vdfileBytes);
                    fileVideoContent.Headers.ContentType = MediaTypeHeaderValue.Parse("video/mp4");
                    formData.Add(fileVideoContent, "video", System.IO.Path.GetFileName(videofilePath));

                    // Gửi yêu cầu POST
                    HttpResponseMessage response = await client.PostAsync(url, formData);

                    return response;
                }
            }
        }

        private void SaveVisualAsHighDpiImage(Visual visual, string filePath, int dpi = 600)
        {
            double scale = dpi / 96.0;

            Rect bounds = VisualTreeHelper.GetDescendantBounds(visual);
            int width = (int)(bounds.Width * scale);
            int height = (int)(bounds.Height * scale);

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

        private Visual DrawToImage()
        {
            var layout = _db.LayoutApp.AsNoTracking().FirstOrDefault(x => x.IsSelected);

            if (layout == null) return null;
            var svgInfo = _db.SvgInfors.Include(x => x.SvgRectTags)
                .AsNoTracking()
                .FirstOrDefault(x => x.Name == layout.SVGMappingName);

            var itemQR = svgInfo.SvgRectTags.SingleOrDefault(x => x.IsQRRect);
            var image = dicImages.Values.FirstOrDefault(x => ((dynamic)x.DataContext).isQR);

            DrawingVisual visual = new DrawingVisual();
            using (DrawingContext dc = visual.RenderOpen())
            {
                double x = Canvas.GetLeft(gridImages);
                double y = Canvas.GetTop(gridImages);
                dc.DrawRectangle(new VisualBrush(gridImages), null, new Rect(0, 0, svgInfo!.ActualWidth, svgInfo!.ActualHeight));
                dc.DrawImage(image!.Source, new Rect(itemQR!.X.ToDouble(), itemQR!.Y.ToDouble(), itemQR!.Width.ToDouble(), itemQR!.Height.ToDouble()));
            }

            SaveVisualAsHighDpiImage(visual, $"{layout.ImageFolderPath}/prints/image.png");
            return visual;
        }
    }
}
