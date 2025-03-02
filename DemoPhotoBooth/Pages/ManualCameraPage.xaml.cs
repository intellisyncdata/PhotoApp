using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
using System.Windows.Shell;
using DemoPhotoBooth.Common;
using DemoPhotoBooth.DataContext;
using DemoPhotoBooth.Models;
using DemoPhotoBooth.Pages.Preview;
using EOSDigital.API;
using EOSDigital.SDK;
using Microsoft.EntityFrameworkCore;
using PhotoboothWpf;
using Camera = EOSDigital.API.Camera;

namespace DemoPhotoBooth.Pages
{
    /// <summary>
    /// Interaction logic for ManualCameraPage.xaml
    /// </summary>
    public partial class ManualCameraPage : Page
    {
        private CanonAPI APIHandler;
        private Camera MainCamera;
        ImageBrush liveView = new ImageBrush();
        private CommonDbDataContext _db;
        private Action<BitmapSource> SetImageAction;
        List<Camera> CamList;
        private LiveViewRecorder recorder = new LiveViewRecorder();
        // Default Landscape
        private bool isPortrait = false;

        public int photoNumber = 0;
        public int maxPhotosTaken = 8;
        private int timeLeft = 90;
        private int photosTaken = 0;
        public bool PhotoTaken = false;
        private Layout _layout { get; set; }
        private List<Layout> _listLayouts { get; set; }
        private (string root, string printPath) rootPath = (string.Empty, string.Empty);
        public System.Windows.Threading.DispatcherTimer timeLeftCounter;


        public ManualCameraPage(Layout layout, List<Layout> listLayouts, bool portraitMode = false)
        {
            InitializeComponent();
            ActivateTimers();
            isPortrait = portraitMode;
            _layout = layout;
            _listLayouts = listLayouts;
            SetViewMode();
        }

        private void SetViewMode()
        {
            if (isPortrait)
            {
                var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backgrounds/bgframevertical.png");
                this.MyBackgroundLiveView.Source = new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute));
                this.TimeBoxLive.VerticalAlignment = VerticalAlignment.Center;
            }
            else
            {
                var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backgrounds/bgframehorizontal.png");
                this.MyBackgroundLiveView.Source = new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute));
                this.TimeBoxLive.VerticalAlignment = VerticalAlignment.Top;
            }
        }

        private void ActivateTimers()
        {
            timeLeftCounter = new System.Windows.Threading.DispatcherTimer();
            timeLeftCounter.Tick += ShowTimeLeft;
            timeLeftCounter.Interval = TimeSpan.FromSeconds(1);

        }

        private void ShowTimeLeft(object sender, EventArgs e)
        {
            if (timeLeft > 0)
            {
                CountdownTimer.Text = timeLeft.ToString();
                timeLeft--;
            }
            else
            {
                timeLeftCounter.Stop();
                CountdownTimer.Text = "📸";

                if (photosTaken < maxPhotosTaken)
                {
                    MakePhoto(sender, e);
                }
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _db = new CommonDbDataContext();
                MainCamera?.Dispose();

                // Initialize Canon camera
                rootPath = Create.TodayPhotoFolder();
                APIHandler = new CanonAPI();
                APIHandler.CameraAdded += APIHandler_CameraAdded;

                ErrorHandler.SevereErrorHappened += ErrorHandler_SevereErrorHappened;
                ErrorHandler.NonSevereErrorHappened += ErrorHandler_NonSevereErrorHappened;

                SetImageAction = (BitmapSource img) => { liveView.ImageSource = img; };

                RefreshCamera();
                OpenSession();
                MainCamera?.SetCapacity(4096, 0x1FFFFFFF);

                // Start the photo-taking process
                StartPhotoProcess();

                var layoutInfo = _db.LayoutApp.AsNoTracking().FirstOrDefault(x => x.IsSelected);
                if (layoutInfo != null)
                {
                    layoutInfo.ImageFolderPath = rootPath.root;
                    _db.LayoutApp.Update(layoutInfo);
                    _db.SaveChanges();
                }
            }
            catch (NullReferenceException)
            {
                Report.Error("Check if camera is turned on and restart the program", true);
            }
            catch (DllNotFoundException)
            {
                Report.Error("Canon DLLs not found!", true);
            }
            catch (Exception ex)
            {
                Report.Error(ex.Message, true);
            }

        }

        private async void StartPhotoProcess()
        {
            photosTaken = 0;

            recorder.StartRecording();  // Start recording before taking photos

            // Wait until MainCamera.IsLiveViewOn becomes true
            while (!MainCamera.IsLiveViewOn)
            {
                await Task.Delay(1000); // Wait for 1 second
            }

            TakePhoto();  // Start the first countdown/photo cycle
        }

        private void TakePhoto()
        {

            CountdownTimer.Visibility = Visibility.Visible;
            PhotosLeftCounter.Text = $"{photosTaken}/{maxPhotosTaken}";
            timeLeftCounter.Start();
        }
        private async void MakePhoto(object sender, EventArgs e)
        {
            try
            {
                #region Take Photo
                Debug.WriteLine($"Taking photo {photosTaken}...");
                // Simulating camera photo-taking commands

                MainCamera.TakePhotoAsync();
                //MainCamera.SendCommand(CameraCommand.PressShutterButton, (int)ShutterButton.Halfway);
                //// Simulate half press delay
                //await Task.Delay(500);

                //MainCamera.SendCommand(CameraCommand.PressShutterButton, (int)ShutterButton.Completely);
                //// Simulate capture delay
                //await Task.Delay(500);

                //MainCamera.SendCommand(CameraCommand.PressShutterButton, (int)ShutterButton.OFF);
                Debug.WriteLine("Photo taken successfully.");
                #endregion Take Photo

                // Simulate photo saving delay asynchronously
                await Task.Delay(2000);

                CountdownTimer.Text = "📸";

                // Wait asynchronously for photo saving confirmation
                await WaitForPhotoToBeSaved();

                if (photosTaken < maxPhotosTaken)
                {
                    // Start the next countdown cycle
                    MakePhoto(sender, e);
                }
                else
                {
                    Debug.WriteLine("Photo session completed.");
                    CountdownTimer.Text = "Đã chụp xong!";
                    // Stop recording when all photos are taken
                    recorder.StopRecording();
                    NavigationService?.Navigate(new NewPreviewPage(_layout, _listLayouts, isPortrait));
                }
            }
            catch (Exception ex)
            {
                Report.Error(ex.Message, false);
            }
        }

        private async Task WaitForPhotoToBeSaved()
        {
            while (!PhotoTaken)
            {
                // Asynchronously wait for the photo to be saved
                await Task.Delay(1000);
            }
            PhotoTaken = false;
        }

        private void APIHandler_CameraAdded(CanonAPI sender)
        {
            try
            {
                Dispatcher.Invoke((Action)delegate
                {
                    RefreshCamera();
                });
            }
            catch (Exception ex)
            {
                Report.Error(ex.Message, false);
            }
        }

        private void RefreshCamera()
        {
            CameraListBox.Items.Clear();
            CamList = APIHandler.GetCameraList();
            foreach (Camera cam in CamList) CameraListBox.Items.Add(cam.DeviceName);
            if (MainCamera?.SessionOpen == true) CameraListBox.SelectedIndex = CamList.FindIndex(t => t.ID == MainCamera.ID);
            else if (CamList.Count > 0) CameraListBox.SelectedIndex = 0;
        }

        private void ErrorHandler_NonSevereErrorHappened(object sender, ErrorCode ex)
        {
            string errorCode = ((int)ex).ToString("X");
            switch (errorCode)
            {
                case "8D01": // TAKE_PICTURE_AF_NG
                    if (photosTaken != 0)
                    {
                        photosTaken--;
                    }
                    PhotoTaken = true;
                    Debug.WriteLine("Autofocus error");
                    return;
            }
            Report.Error($"SDK Error code: {ex} ({((int)ex).ToString("X")})", false);
        }

        private void ErrorHandler_SevereErrorHappened(object sender, Exception ex)
        {
            Report.Error(ex.Message, true);
        }

        private void OpenSession()
        {
            try
            {
                if (MainCamera?.SessionOpen == true) CloseSession();
                else
                {
                    if (CameraListBox.SelectedIndex >= 0)
                    {
                        MainCamera = CamList[CameraListBox.SelectedIndex];
                        MainCamera.OpenSession();
                        MainCamera.LiveViewUpdated += MainCamera_LiveViewUpdated;
                        MainCamera.StateChanged += MainCamera_StateChanged;
                        MainCamera.DownloadReady += MainCamera_DownloadReady;
                        MainCamera.SetSetting(PropertyID.SaveTo, (int)SaveTo.Host);
                        MainCamera?.SetCapacity(4096, int.MaxValue);
                        StartLiveView();
                        MainCamera?.SetSetting(PropertyID.Evf_OutputDevice, (int)(EvfOutputDevice.PC | EvfOutputDevice.Camera));

                    }
                }
            }
            catch (Exception ex) { Report.Error(ex.Message, false); }

        }

        private void MainCamera_StateChanged(Camera sender, StateEventID eventID, int parameter)
        {
            try
            {
                if (eventID == StateEventID.Shutdown)
                {
                    Dispatcher.Invoke((Action)delegate
                    {
                        CloseSession();
                    });
                }
            }
            catch (Exception ex)
            {
                //Report.Error(ex.Message, false); 
            }
        }

        private void CloseSession()
        {
            try
            {
                MainCamera.CloseSession();
            }
            catch (ObjectDisposedException) { Report.Error("Camera has been turned off! \nPlease turned it on and restart the application", true); }
        }

        #region Live view

        private void StartLiveView()
        {
            Slider.Visibility = Visibility.Visible;
            CountdownTimer.Visibility = Visibility.Visible;
            try
            {
                MainCamera.SendCommand(CameraCommand.PressShutterButton, 1);

            }
            finally
            {
                MainCamera.SendCommand(CameraCommand.PressShutterButton, 0);
            }
            try
            {

                Slider.Background = liveView;
                MainCamera.StartLiveView();
            }
            catch (Exception ex) { Report.Error(ex.Message, false); }
        }

        #endregion
        private void MainCamera_LiveViewUpdated(Camera sender, Stream img)
        {
            try
            {
                using (WrapStream s = new WrapStream(img))
                {
                    img.Position = 0;
                    BitmapImage EvfImage = new BitmapImage();
                    EvfImage.BeginInit();
                    EvfImage.StreamSource = s;
                    EvfImage.CacheOption = BitmapCacheOption.OnLoad;
                    EvfImage.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                    EvfImage.DecodePixelWidth = 1920; // Ensuring Full HD width
                    EvfImage.DecodePixelHeight = 1080; // Ensuring Full HD height 
                    EvfImage.EndInit();
                    EvfImage.Freeze();

                    // Apply horizontal flip transformation
                    //TransformedBitmap flippedImage = new TransformedBitmap();
                    //flippedImage.BeginInit();
                    //flippedImage.Source = EvfImage;
                    //flippedImage.Transform = new ScaleTransform(-1, 1, EvfImage.PixelWidth / 2.0, 0);
                    //flippedImage.EndInit();
                    //flippedImage.Freeze();

                    // Update UI
                    Application.Current.Dispatcher.BeginInvoke(SetImageAction, EvfImage);

                    // Capture frame for video
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        recorder.CaptureFrame(EvfImage);
                    }));
                }
            }
            catch (Exception ex)
            {
                //Report.Error(ex.Message, false);
            }
        }

        private void MainCamera_DownloadReady(Camera sender, DownloadInfo Info)
        {
            string tempPath = string.Empty;
            try
            {
                if (photosTaken < maxPhotosTaken)
                {
                   
                    photoNumber++;
                    var savedata = new SavePhoto(photoNumber);
                    string dir = savedata.FolderDirectory;

                    Info.FileName = savedata.PhotoName;
                    tempPath = System.IO.Path.Combine(dir, Info.FileName);
                    sender.DownloadFile(Info, dir);

                    var layoutApp = _db.LayoutApp.FirstOrDefault();
                    if (layoutApp.FrameType == "vertical")
                    {
                        ReSize.CropAndSaveImage(savedata.PhotoDirectory, Info.FileName);
                    }

                    Debug.WriteLine($"Capturing photo {photosTaken + 1} from remote trigger");
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        PhotosLeftCounter.Text = $"{photosTaken}/{maxPhotosTaken}";
                    }));

                    if (photosTaken == 7)
                    {
                        photosTaken = 0;
                        photoNumber = 0;
                        recorder.StopRecording();
                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            var mainWindow = Application.Current.MainWindow as MainWindow;
                            if (mainWindow != null)
                            {
                                // Nếu MainWindow có Frame để điều hướng, sử dụng Frame thay vì mở cửa sổ mới
                                var frame = mainWindow.FindName("MainFrame") as Frame;
                                if (frame != null)
                                {
                                    frame.Navigate(new NewPreviewPage(_layout, _listLayouts, isPortrait));
                                }
                                else
                                {
                                    // Nếu không có Frame, mở cửa sổ mới nhưng đặt kích thước giống MainWindow
                                    var previewPage = new NewPreviewPage(_layout, _listLayouts, isPortrait);
                                    var window = new Window
                                    {
                                        Content = previewPage,
                                        Width = mainWindow.Width,  // Giữ kích thước MainWindow
                                        Height = mainWindow.Height,
                                        WindowStartupLocation = WindowStartupLocation.CenterScreen
                                    };
                                    window.Show();
                                }
                            }
                        }));
                    }
                    photosTaken++;
                }
                else
                {
                    Debug.WriteLine("Maximum number of photos taken. Navigating to preview page...");

                    recorder.StopRecording();

                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        var mainWindow = Application.Current.MainWindow as MainWindow;
                        if (mainWindow != null)
                        {
                            // Nếu MainWindow có Frame để điều hướng, sử dụng Frame thay vì mở cửa sổ mới
                            var frame = mainWindow.FindName("MainFrame") as Frame;
                            if (frame != null)
                            {
                                frame.Navigate(new NewPreviewPage(_layout, _listLayouts, isPortrait));
                            }
                            else
                            {
                                // Nếu không có Frame, mở cửa sổ mới nhưng đặt kích thước giống MainWindow
                                var previewPage = new NewPreviewPage(_layout, _listLayouts, isPortrait);
                                var window = new Window
                                {
                                    Content = previewPage,
                                    Width = mainWindow.Width,  // Giữ kích thước MainWindow
                                    Height = mainWindow.Height,
                                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                                };
                                window.Show();
                            }
                        }
                    }));
                }
            }
            catch (Exception ex)
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
                //Report.Error(ex.Message, false);
            }

            PhotoTaken = true;

        }


        private void NavigatePreviewPage()
        {
            var window = System.Windows.Application.Current.MainWindow as MainWindow;
            if (window != null)
            {
                window.MainFrame.Navigate(new NewPreviewPage(_layout, _listLayouts, isPortrait));
            }
        }
    }
}
