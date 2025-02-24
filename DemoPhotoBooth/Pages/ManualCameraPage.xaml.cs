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
using DemoPhotoBooth.Common;
using DemoPhotoBooth.DataContext;
using DemoPhotoBooth.Models;
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
        private bool isManual = false;

        public int photoNumber = 0;
        public int maxPhotosTaken = 8;
        private int timeLeft = 10;
        private int timeLeftCopy = 10;
        private int photosTaken = 0;
        public bool PhotoTaken = false;
        private Layout _layout { get; set; }
        private List<Layout> _listLayouts { get; set; }
        private (string root, string printPath) rootPath = (string.Empty, string.Empty);
        public System.Windows.Threading.DispatcherTimer betweenPhotos;
        public System.Windows.Threading.DispatcherTimer secondCounter;


        public ManualCameraPage(Layout layout, List<Layout> listLayouts, bool portraitMode = false)
        {
            InitializeComponent();
            isPortrait = portraitMode;
            _layout = layout;
            _listLayouts = listLayouts;
            SetViewMode();
        }

        private void SetViewMode()
        {
            if (isPortrait)
            {
                var path = "pack://application:,,,/Layouts/bg-vertical.png";
                this.MyBackgroundLiveView.Source = new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute));
                this.TimeBoxLive.VerticalAlignment = VerticalAlignment.Center;
            }
            else
            {
                var path = "pack://application:,,,/Layouts/bg-horizontal.png";
                this.MyBackgroundLiveView.Source = new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute));
                this.TimeBoxLive.VerticalAlignment = VerticalAlignment.Top;
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

        private void StartPhotoProcess()
        {
            photosTaken = 0;

            recorder.StartRecording();  // Start recording before taking photos

            TakePhoto();  // Start the first countdown/photo cycle
        }

        private void TakePhoto()
        {
            if (photosTaken < maxPhotosTaken)
            {
                Debug.WriteLine($"Starting countdown for photo {photosTaken + 1}");
                timeLeftCopy = timeLeft;
                CountdownTimer.Visibility = Visibility.Visible;
                PhotosLeftCounter.Text = $"{photosTaken + 1}/{maxPhotosTaken}";
                secondCounter.Start();
                betweenPhotos.Start();
            }
            else
            {
                Debug.WriteLine("All photos have been taken.");
            }
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
                else if (eventID == StateEventID.WillSoonShutDown)
                {
                    Dispatcher.Invoke((Action)delegate
                    {
                        CapturePhotoFromRemote();
                    });
                }
            }
            catch (Exception ex)
            {
                //Report.Error(ex.Message, false); 
            }
        }

        private void CapturePhotoFromRemote()
        {
            try
            {
                if (photosTaken < maxPhotosTaken)
                {
                    Debug.WriteLine($"Capturing photo {photosTaken + 1} from remote trigger");
                    MainCamera.TakePhoto();
                    photosTaken++;
                }
                else
                {
                    Debug.WriteLine("Maximum number of photos taken.");
                }
            }
            catch (Exception ex)
            {
                Report.Error(ex.Message, false);
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
                    TransformedBitmap flippedImage = new TransformedBitmap();
                    flippedImage.BeginInit();
                    flippedImage.Source = EvfImage;
                    flippedImage.Transform = new ScaleTransform(-1, 1, EvfImage.PixelWidth / 2.0, 0);
                    flippedImage.EndInit();
                    flippedImage.Freeze();

                    // Update UI
                    Application.Current.Dispatcher.BeginInvoke(SetImageAction, flippedImage);

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

            try
            {
                photoNumber++;
                var savedata = new SavePhoto(photoNumber);
                string dir = savedata.FolderDirectory;

                Info.FileName = savedata.PhotoName;
                sender.DownloadFile(Info, dir);
                if (isPortrait)
                {
                    ReSize.CropAndSaveImage(savedata.PhotoDirectory, photoNumber);
                }
            }
            catch (Exception ex)
            {
                //Report.Error(ex.Message, false);
            }

            PhotoTaken = true;

        }
    }
}
