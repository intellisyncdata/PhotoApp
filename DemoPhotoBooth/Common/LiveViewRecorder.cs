using System;
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
        private int frameWidth = 426;
        private int frameHeight = 240;
        private int fps = 10; // Giảm FPS để tạo hiệu ứng timelapse
        private DateTime lastFrameTime;
        private int captureInterval = 1000; // Mili-giây (1000ms = 1 giây)

        public LiveViewRecorder()
        {
            string currentPath = Environment.CurrentDirectory;
            string dateTimePath = DateTime.Now.ToString("yyyyMMdd_HHmmss");
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
            lastFrameTime = DateTime.Now;

            // Khởi tạo VideoWriter với FPS thấp để tạo hiệu ứng timelapse
            videoWriter = new VideoWriter(videoOutputPath, FourCC.H264, fps, new OpenCvSharp.Size(frameWidth, frameHeight));

            if (!videoWriter.IsOpened())
            {
                Debug.WriteLine("Failed to open video writer.");
                isRecording = false;
            }

            Debug.WriteLine("Timelapse recording started...");
        }

        public void StopRecording()
        {
            if (!isRecording) return;

            isRecording = false;
            videoWriter?.Release();
            Debug.WriteLine($"Timelapse recording stopped. Video saved at: {videoOutputPath}");
        }

        public void CaptureFrame(BitmapImage image)
        {
            if (!isRecording || videoWriter == null) return;

            DateTime now = DateTime.Now;
            if ((now - lastFrameTime).TotalMilliseconds < captureInterval)
            {
                return;
            }

            lastFrameTime = now;

            try
            {
                Mat frame = BitmapImageToMat(image);
                Cv2.Resize(frame, frame, new OpenCvSharp.Size(frameWidth, frameHeight));

                videoWriter.Write(frame);
                frame.Dispose();
                Debug.WriteLine("Frame captured for timelapse.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error capturing frame: {ex.Message}");
            }
        }

        private Mat BitmapImageToMat(BitmapImage bitmap)
        {
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
