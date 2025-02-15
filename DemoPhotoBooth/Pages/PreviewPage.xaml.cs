using DemoPhotoBooth.DataContext;
using DemoPhotoBooth.Models;
using DemoPhotoBooth.Models.DTOs;
using DemoPhotoBooth.Pages.Preview;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;
using Path = System.IO.Path;

namespace DemoPhotoBooth.Pages
{
    /// <summary>
    /// Interaction logic for PreviewPage.xaml
    /// </summary>
    public partial class PreviewPage : Page
    {
        private Dictionary<string, List<object>> dicControls = new Dictionary<string, List<object>>();
        private uint countSelected = 0;
        private DispatcherTimer dispatcherTimer = new DispatcherTimer();
        private uint timeStep = 120; // 2 mins
        private CommonDbDataContext _db;
        private string imageFolderPath = string.Empty;
        private static readonly List<string> extensions = new List<string>
        {
            ".jpg",
            ".png",
            ".jpeg"
        };
        private string layoutName = string.Empty;
        private Layout Layout;
        private Dictionary<string, PreviewGrid> dicGridView;
        private PreviewPartial _previewPartial;
        private string pathLayoutTemp = string.Empty;
        private int quantity = 4;
        public PreviewPage()
        {
            InitializeComponent();
            _db = new CommonDbDataContext();
            LoadLayoutInfo();
            //#if DEBUG
            //            imageFolderPath = $"{Directory.GetCurrentDirectory()}/20.01.2025";
            //#endif  
           // imageFolderPath = $"{Directory.GetCurrentDirectory()}/{DateTime.Now:dd.MM.yyyy}";
            DrawControls();
            LoadLayout();
            btnReset.Visibility = Visibility.Hidden;
            btnContinue.Visibility = Visibility.Hidden;
            dicGridView = new Dictionary<string, PreviewGrid>();
        }

        private BitmapSource LoadAndFixOrientation(string filePath)
        {
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                var decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                var original = decoder.Frames[0];

                var metadata = original.Metadata as BitmapMetadata;
                if (metadata != null && metadata.ContainsQuery("System.Photo.Orientation"))
                {
                    var orientation = (ushort)metadata.GetQuery("System.Photo.Orientation");

                    var transform = new RotateTransform(0);
                    switch (orientation)
                    {
                        case 3: transform = new RotateTransform(180); break;
                        case 6: transform = new RotateTransform(90); break;
                        case 8: transform = new RotateTransform(270); break;
                    }

                    var transformedBitmap = new TransformedBitmap(original, transform);
                    return transformedBitmap;
                }

                return original;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();

            var svgInfo = _db.SvgInfors.AsNoTracking()
                .Include(x=>x.SvgRectTags)
                .FirstOrDefault(x => x.Name == layoutName);


            if (svgInfo != null)
            {
                dicGridView = svgInfo.SvgRectTags
                    .Where(x => !x.IsQRRect)
                    .ToDictionary(x => x.No.ToString(), x => new PreviewGrid
                    {
                        X = x.X,
                        Y = x.Y,
                        Width = x.Width,
                        Height = x.Height,
                        Uri = string.Empty
                    });


                var layoutSelected = _db.LayoutApp
                    .AsNoTracking()
                    .SingleOrDefault(x => x.IsSelected);

                if (layoutSelected != null) 
                {
                    quantity = layoutSelected.Quantity ?? 4;
                    Dispatcher.Invoke(() => lblCountImages.Text = $"0/{quantity}");
                }
            }

            Layout = CreateLayoutTemp();
        }

        private void DispatcherTimer_Tick(object? sender, EventArgs e)
        {
            if (timeStep > 0)
            {
                lblTimer.Text = $"{timeStep--}";
                return;
            }
            dispatcherTimer.Stop();
            NextDownloadPage();
        }

        private void NextDownloadPage()
        {
            NavigationService.Navigate(new PrintAndDownloadPage());
        }

        private void LoadLayout()
        {
            _previewPartial = new PreviewPartial();
            frmLayout.Navigate(_previewPartial);
        }

        private void LoadLayoutInfo()
        {
            var item = _db.LayoutApp.AsNoTracking().FirstOrDefault(x => x.IsSelected);
            if (item == null)
            {
                return;
            }
            imageFolderPath = item.ImageFolderPath ?? string.Empty;
            pathLayoutTemp = $"{Directory.GetCurrentDirectory()}/Layouts/Temp/{item.SVGMappingName}";
        }

        private Layout CreateLayoutTemp()
        {
            var layout = new Layout();
            if (string.IsNullOrEmpty(layoutName)) return layout;
            if (!Directory.Exists($"{Directory.GetCurrentDirectory()}/Layouts/Temp"))
            {
                Directory.CreateDirectory($"{Directory.GetCurrentDirectory()}/Layouts/Temp");
            }
            if (File.Exists($"{Directory.GetCurrentDirectory()}/Layouts/Temp/{layoutName}"))
            {
                File.Delete($"{Directory.GetCurrentDirectory()}/Layouts/Temp/{layoutName}");
            }
            var path = $"{Directory.GetCurrentDirectory()}/Layouts/{layoutName}";
            var destPth = $"{Directory.GetCurrentDirectory()}/Layouts/Temp/{layoutName}";

            System.IO.File.Copy(path, destPth);

            layout.ImageUrl = destPth;
            return layout;
        }

        private void DrawControls()
        {
            if (!Directory.Exists(imageFolderPath)) { return; }

            int i = 0;
            int j = 0;

            var files = Directory.GetFiles(imageFolderPath)?.ToList() ?? new List<string>();
            files = files.Where(x => extensions.Contains(Path.GetExtension(x).ToLower())).ToList();
            foreach (var item in files)
            {
                if (j >= 2)
                {
                    i++;
                    j = 0;
                }
                var panel = DrawPanel(i, j);
                var image = DrawImage(item);
                var rect = DrawRectangle();
                var textBlock = DrawTextBlock();
                var subGrid = DrawGrid(i, j, image, rect, textBlock);
                panel.Children.Add(subGrid);
                gridImages.Children.Add(panel);
                j++;
            }
        }

        private StackPanel DrawPanel(int i, int j)
        {
            var panel = new StackPanel();
            //panel.Width = 280;
            //panel.Height = 180;
            //panel.MaxWidth = 280;
            //panel.MaxHeight = 180;
            panel.Background = new SolidColorBrush(Colors.WhiteSmoke);
            panel.Margin = new Thickness(10, 0, 0, 10);
            Grid.SetRow(panel, i);
            Grid.SetColumn(panel, j);

            return panel;
        }

        private Grid DrawGrid(int i, int j, Image imageControl, Rectangle rect, TextBlock textBlock)
        {
            var groupGrid = new Grid();
            groupGrid.Name = $"Grid{i}{j}";
            groupGrid.Children.Add(imageControl);
            groupGrid.Children.Add(rect);
            groupGrid.Children.Add(textBlock);
            dicControls.Add(groupGrid.Name, new List<object>
            {
                rect,
                textBlock,
                imageControl
            });
            groupGrid.MouseLeftButtonDown += GroupGrid_MouseLeftButtonDown;
            groupGrid.TouchDown += GroupGrid_TouchDown;

            return groupGrid;
        }

        private Image DrawImage(string path)
        {
            var newImageControl = new Image();
            LoadImageFromFile(path, newImageControl);

            return newImageControl;
        }

        private Rectangle DrawRectangle()
        {
            var rect = new Rectangle();

            rect.HorizontalAlignment = HorizontalAlignment.Stretch;
            rect.VerticalAlignment = VerticalAlignment.Stretch;
            rect.Opacity = 0;
            rect.Fill = new SolidColorBrush(Colors.Black);

            return rect;
        }

        private TextBlock DrawTextBlock()
        {
            var textBlb = new TextBlock();
            textBlb.FontSize = 58;
            textBlb.Foreground = new SolidColorBrush(Colors.White);
            textBlb.HorizontalAlignment = HorizontalAlignment.Center;
            textBlb.VerticalAlignment = VerticalAlignment.Center;
            textBlb.Opacity = 0;
            textBlb.Text = "1";
            textBlb.FontFamily = new FontFamily("Time New Roman");

            return textBlb;
        }


        private void LoadImageFromFile(string filePath, Image imageControl)
        {
            var correctedBitmap = LoadAndFixOrientation(filePath);

            imageControl.Source = correctedBitmap;
            var uri = new System.Uri(filePath);
            imageControl.Uid = uri.AbsoluteUri;
            imageControl.Stretch = Stretch.UniformToFill;
            imageControl.VerticalAlignment = VerticalAlignment.Center;
            imageControl.HorizontalAlignment = HorizontalAlignment.Center;
        }

        private List<ImageSource> imageSources = new List<ImageSource>();

        private void ModifyLayoutTemp(string uri)
        {
            var gridItem = dicGridView.Values.FirstOrDefault(x => x.Uri == string.Empty);

            if (gridItem != null) 
            {
                gridItem.Uri = uri;

                XDocument doc = XDocument.Load(pathLayoutTemp);
                var ns = doc.Root.Name.Namespace;
                XElement image = new XElement(ns + "image",
                    new XAttribute("href", uri),
                    new XAttribute("x", $"{gridItem.X}"),
                    new XAttribute("y", $"{gridItem.Y}"),
                    new XAttribute("width", $"{gridItem.Width}"),
                    new XAttribute("height", $"{gridItem.Height}")
                );

                doc.Root.Add(image);
                doc.Save(pathLayoutTemp);
            }
        }

        private void HandleMultiSelectImages(object sender)
        {
            var grid = sender as Grid;

            if (grid is null) return;
            dicControls.TryGetValue(grid.Name, out List<object>? lstControls);

            if (lstControls is null || countSelected == quantity) return;

            foreach (var item in lstControls)
            {
                if (item is Rectangle)
                {
                    var rect = item as Rectangle;
                    if (rect is null) return;
                    if (rect.Opacity == 0.5) return;

                    rect.Opacity = 0.5;

                    ++countSelected;
                }
                else if (item is TextBlock)
                {
                    var txtBlock = item as TextBlock;
                    if (txtBlock is null) return;
                    if (txtBlock.Opacity == 1) return;

                    txtBlock.Opacity = 1;
                }
                else
                {
                    var image = item as Image;
                    if (image is null) return;

                    // send Image
                    ModifyLayoutTemp(image.Uid);
                    _previewPartial.ReAssignLayout(new Layout
                    {
                        ImageUrl = new Uri(pathLayoutTemp).AbsoluteUri
                    });
                }
            }
            lblCountImages.Text = $"{countSelected}/{quantity}";

            if (countSelected != 0)
            {
                btnReset.Visibility = Visibility.Visible;
            }

            if (countSelected == quantity) 
            {
                btnContinue.Visibility = Visibility.Visible;
            }
        }

        private void btnContinue_Click(object sender, RoutedEventArgs e)
        {
            dispatcherTimer.Stop();
            NextDownloadPage();
        }

        private void GroupGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            HandleMultiSelectImages(sender);
        }

        private void GroupGrid_TouchDown(object? sender, TouchEventArgs e)
        {
            if (sender is null) return;

            HandleMultiSelectImages(sender);
        }

        private void ResetLayout()
        {
            XDocument doc = XDocument.Load(pathLayoutTemp);
            var imageTags = doc.Root.Elements().Where(x => x.Name.LocalName == "image").ToList();

            if (imageTags.Any()) 
            {
                imageTags.ForEach(x => x.Remove());
            }

            doc.Save(pathLayoutTemp);

            dicGridView.Values.ToList().ForEach(x => x.Uri = string.Empty);
            _previewPartial.ReAssignLayout(new Layout
            {
                ImageUrl = new Uri(pathLayoutTemp).AbsoluteUri
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            countSelected = 0;
            lblCountImages.Text = $"0/{quantity}";

            foreach (var item in dicControls.Values)
            {
                foreach (var control in item)
                {
                    if (control is Rectangle)
                    {
                        var rect = control as Rectangle;
                        if (rect is null) return;

                        rect.Opacity = 0;
                    }
                    else if (control is TextBlock)
                    {
                        var txtBlock = control as TextBlock;
                        if (txtBlock is null) return;

                        txtBlock.Opacity = 0;
                    }
                }
            }
            ResetLayout();
            btnReset.Visibility = Visibility.Hidden;
            btnContinue.Visibility = Visibility.Hidden;
        }
    }
}
