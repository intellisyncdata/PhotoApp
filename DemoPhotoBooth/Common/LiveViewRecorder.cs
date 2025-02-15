using System.IO;
using OpenCvSharp;
using System.Windows.Media.Imaging;
using System.Diagnostics;

namespace DemoPhotoBooth.Common
{
    public class LiveViewRecorder
    {
        private bool isRecording = false;
        private VideoWriter videoWriter;
        private string videoOutputPath;
        private int frameWidth = 426; // Set resolution based on your needs
        private int frameHeight = 240;
        private int fps = 30;

        public LiveViewRecorder()
        {
            string currentPath = Environment.CurrentDirectory;
            string dateTimePath = Actual.DateNow();
            string pathString = Path.Combine(currentPath, dateTimePath);
            Directory.CreateDirectory(pathString);
            string videoPath = "video";
            pathString = Path.Combine(dateTimePath, videoPath);
            Directory.CreateDirectory(pathString);
            videoOutputPath = Path.Combine(pathString, $"recording.mp4");
        }

        public void StartRecording()
        {
            if (isRecording) return;

            isRecording = true;

            // Initialize OpenCV VideoWriter
            videoWriter = new VideoWriter(videoOutputPath, FourCC.H264, fps, new OpenCvSharp.Size(frameWidth, frameHeight));

            if (!videoWriter.IsOpened())
            {
                Debug.WriteLine("Failed to open video writer.");
                isRecording = false;
            }

            Debug.WriteLine("Recording started...");
        }

        public void StopRecording()
        {
            if (!isRecording) return;

            isRecording = false;
            videoWriter?.Release();
            Debug.WriteLine($"Recording stopped. Video saved at: {videoOutputPath}");
        }

        public void CaptureFrame(BitmapImage image)
        {
            if (!isRecording || videoWriter == null) return;

            try
            {
                // Convert BitmapImage to OpenCV Mat
                Mat frame = BitmapImageToMat(image);

                // Resize to ensure dimensions match video settings
                Cv2.Resize(frame, frame, new OpenCvSharp.Size(frameWidth, frameHeight));

                // Write frame to video
                videoWriter.Write(frame);
                frame.Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error capturing frame: {ex.Message}");
            }
        }

        private Mat BitmapImageToMat(BitmapImage bitmap)
        {
            // Convert WPF BitmapImage to OpenCV Mat
            using (MemoryStream ms = new MemoryStream())
            {
                BitmapEncoder encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                encoder.Save(ms);
                return Mat.FromImageData(ms.ToArray(), ImreadModes.Color);
            }
        }
    }
}
