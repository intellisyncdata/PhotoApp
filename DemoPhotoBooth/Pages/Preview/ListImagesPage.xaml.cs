using DemoPhotoBooth.DataContext;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Documents;
using System.Windows.Input;
using DemoPhotoBooth.Models;
using OpenCvSharp;
using System.Xml.Linq;
using DemoPhotoBooth.Models.DTOs;
using System;
using DemoPhotoBooth.Communicate;
using static DemoPhotoBooth.Communicate.MessageTypes;
using System.Drawing;
using Image = System.Windows.Controls.Image;
using SixLabors.ImageSharp;
using static System.Net.Mime.MediaTypeNames;

namespace DemoPhotoBooth.Pages.Preview
{
    /// <summary>
    /// Interaction logic for ListImagesPage.xaml
    /// </summary>
    public partial class ListImagesPage : Page
    {
        private Dictionary<string, List<object>> dicControls = new Dictionary<string, List<object>>();
        private uint countSelected = 0;
        private int quantity = 4;
        private readonly CommonDbDataContext _db;
        private string imageFolder = string.Empty;
        private string layoutName = string.Empty;
        private TextBlock lblCountImages;
        private string pathLayoutTemp = string.Empty;
        private Dictionary<string, PreviewGrid> dicGridView;
        bool isPotrait = false;
        private static readonly List<string> extensions = new List<string>
        {
            ".jpg",
            ".png",
            ".jpeg"
        };

        public class ImageItem
        {
            public int Row { get; set; }
            public int Column { get; set; }
            public string ImagePath { get; set; }
            public string Name { get; set; }
        }

        public ListImagesPage(bool portraitMode = false)
        {
            InitializeComponent();
            _db = new CommonDbDataContext();
            gridImages.RowDefinitions.Clear();
            gridImages.ColumnDefinitions.Clear();
            isPotrait = portraitMode;
            int Width = 250;
            int Height = 170;
            BorderGridImage.Width = 770;
            BorderGridImage.Height = 530;
            if (isPotrait)
            {
                Width = 170;
                Height = 250;
                BorderGridImage.Width = 530;
                BorderGridImage.Height = 770;
            }
            for (int i = 0; i < 3; i++)
            {
                gridImages.RowDefinitions.Add(new RowDefinition { Height = new GridLength(Height) });
            }
            for (int i = 0; i < 3; i++)
            {
                gridImages.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(Width) });
            }
            LeftSide();
        }

        private void LeftSide()
        {
            var layout = _db.LayoutApp.AsNoTracking().SingleOrDefault(x => x.IsSelected);

            if (layout != null)
            {
                imageFolder = layout.ImageFolderPath ?? string.Empty;
                layoutName = layout.SVGMappingName ?? string.Empty;

                DrawControls(imageFolder);
            }
        }

        private void DrawControls(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath)) return;
            if (!Directory.Exists(folderPath)) return;

            var files = Directory.GetFiles(folderPath).ToList();

            if (files != null && files.Any())
            {
                files = files.Where(x => extensions.Contains(System.IO.Path.GetExtension(x).ToLower())).ToList();

                if (!files.Any()) return;

                int totalImages = Math.Min(files.Count, 8);
                int maxImages = 9;

                int i = 0;
                int j = 0;

                for (int index = 0; index < maxImages; index++)
                {
                    if (j >= 3)
                    {
                        i++;
                        j = 0;
                    }

                    var panel = DrawPanel(i, j);
                    Image image = null;
                    System.Windows.Shapes.Rectangle rect = null;
                    TextBlock textBlock = null;

                    if (index < totalImages)
                    {
                        image = DrawImage(files[index]);
                        rect = DrawRectangle();
                        textBlock = DrawTextBlock();
                    }
                    else if (index == 8)
                    {
                        lblCountImages = DrawTextBlock();
                        textBlock = lblCountImages;
                        lblCountImages.Text = $"0/{quantity}";
                        lblCountImages.Opacity = 1;
                    }

                    var subGrid = DrawGrid(i, j, image, rect, textBlock);
                    panel.Children.Add(subGrid);
                    gridImages.Children.Add(panel);
                    j++;
                }
            }
        }

        private Grid DrawGrid(int i, int j, Image imageControl, System.Windows.Shapes.Rectangle rect, TextBlock textBlock)
        {
            var groupGrid = new Grid();

            if (imageControl != null && rect != null && textBlock != null)
            {
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
            }
            else if (imageControl == null && rect == null && textBlock != null)
            {
                int MarginTop = 40;
                if (isPotrait)
                {
                    MarginTop = 80;
                }
                var textContainer = new Grid
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    //Height = 160,
                    //Width = 240,
                    Margin = new Thickness(0, MarginTop, 0, 0)
                };

                textContainer.Children.Add(textBlock);
                Grid.SetRow(textContainer, i);
                Grid.SetColumn(textContainer, j);

                textContainer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                textContainer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                textContainer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

                textContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });


                Grid.SetRow(textBlock, 1);
                Grid.SetColumn(textBlock, 0);
                groupGrid.Children.Add(textContainer);
            }
            return groupGrid;
        }

        private void HandleMultiSelectImages(object sender)
        {
            var grid = sender as Grid;

            if (grid is null) return;
            dicControls.TryGetValue(grid.Name, out List<object>? lstControls);

            if (lstControls is null || countSelected == quantity) return;

            foreach (var item in lstControls)
            {
                if (item is System.Windows.Shapes.Rectangle)
                {
                    var rect = item as System.Windows.Shapes.Rectangle;
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
                else if (item is Image)
                {
                    var image = item as Image;
                    if (image is null) return;
                    image.DataContext = new
                    {
                        uid = new Uri(image.Uid),
                        gridName = grid.Name,
                        rootPath = image.Uid
                    };
                    App.EventAggregator.GetEvent<Page1ToPage2>().Publish(image.DataContext);
                }
            }
            lblCountImages.Text = $"{countSelected}/{quantity}";
        }

        private void RegisterReceiver()
        {
            App.EventAggregator.GetEvent<Page2ToPage1>().Subscribe(obj =>
            {
                dicControls.TryGetValue(obj.name, out List<object> controls);
                if (controls.Any())
                {
                    countSelected--;
                    lblCountImages.Text = $"{countSelected}/{quantity}";
                    foreach (var control in controls)
                    {
                        if (control is System.Windows.Shapes.Rectangle)
                        {
                            var rect = control as System.Windows.Shapes.Rectangle;
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
            });
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

        private TextBlock DrawTextBlock()
        {
            var textBlb = new TextBlock();
            textBlb.FontSize = 60;
            textBlb.FontFamily = new System.Windows.Media.FontFamily(new Uri("pack://application:,,,/"), "./Layouts/fonts/#Coiny");
            textBlb.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(90, 147, 207));
            textBlb.HorizontalAlignment = HorizontalAlignment.Center;
            textBlb.VerticalAlignment = VerticalAlignment.Center;
            textBlb.TextAlignment = TextAlignment.Center;
            //textBlb.Width = 240;
            //textBlb.Height = 160;
            textBlb.Opacity = 0;
            textBlb.Text = string.Empty;

            return textBlb;
        }

        private System.Windows.Shapes.Rectangle DrawRectangle()
        {
            var rect = new System.Windows.Shapes.Rectangle();
            rect.Height = 240;
            rect.Opacity = 0;
            rect.Fill = new SolidColorBrush(Colors.Black);

            return rect;
        }

        private Image DrawImage(string file)
        {
            var newImageControl = new Image();
            LoadImageFromFile(file, newImageControl);

            return newImageControl;
        }

        private void LoadImageFromFile(string filePath, Image imageControl)
        {
            BitmapImage image = new BitmapImage();
            using (var stream = File.OpenRead(filePath))
            {
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream;
                image.EndInit();

                imageControl.Source = image;
                imageControl.Uid = filePath;
                imageControl.Stretch = Stretch.UniformToFill;
                imageControl.VerticalAlignment = VerticalAlignment.Center;
                imageControl.HorizontalAlignment = HorizontalAlignment.Center;
            }
        }

        private Grid DrawPanel(int i, int j)
        {
            var panel = new Grid();
            panel.Background = new SolidColorBrush(Colors.WhiteSmoke);
            panel.Margin = new Thickness(5, 5, 5, 5);
            Grid.SetRow(panel, i);
            Grid.SetColumn(panel, j);

            return panel;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            RegisterReceiver();
            dicGridView = new Dictionary<string, PreviewGrid>();
            var layoutSelected = _db.LayoutApp
                    .AsNoTracking()
                    .SingleOrDefault(x => x.IsSelected);

            if (layoutSelected != null)
            {
                quantity = layoutSelected.Quantity ?? 4;
                pathLayoutTemp = $"{Directory.GetCurrentDirectory()}/Layouts/Temp/{layoutSelected.SVGMappingName}";

                var svgInfo = _db.SvgInfors.AsNoTracking()
                .Include(x => x.SvgRectTags)
                .FirstOrDefault(x => x.Name == layoutSelected.SVGMappingName);

                dicGridView = svgInfo!.SvgRectTags
                    .Where(x => !x.IsQRRect)
                    .ToDictionary(x => x.No.ToString(), x => new PreviewGrid
                    {
                        X = x.X,
                        Y = x.Y,
                        Width = x.Width,
                        Height = x.Height,
                        Uri = string.Empty
                    });
            }
        }
    }
}
