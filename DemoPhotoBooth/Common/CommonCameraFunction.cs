using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace DemoPhotoBooth
{
    class Report
    {
        static public void Error(string message, bool lockdown)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Debug.WriteLine("Error happend");
        }
    }


    class Actual
    {
        static public string DateNow()
        {
            var date1 = DateTime.Now;
            string todayDate = string.Empty;
            todayDate = date1.ToString("dd") + "." + date1.ToString("MM") + "." + "20" + date1.ToString("yy");
            return todayDate;
        }

        static public string FilePath()
        {
            string currentPath = Environment.CurrentDirectory;
            string dateTimePath = Actual.DateNow();
            string pathString = System.IO.Path.Combine(currentPath, dateTimePath);
            return pathString;
        }
    }

    class Control
    {
        static public bool PhotoTemplate(int actualPhotoNumber, int photoInTemplate)
        {
            if (actualPhotoNumber == photoInTemplate) return true;
            else return false;
        }
    }

    class Create
    {
        static public (string rootPath, string printFolderPath) TodayPhotoFolder()
        {
            string currentPath = Environment.CurrentDirectory;
            string dateTimePath = Actual.DateNow();
            string pathString = System.IO.Path.Combine(currentPath, dateTimePath);
            Directory.CreateDirectory(pathString);
            string imagePrintedPath = "prints";
            pathString = System.IO.Path.Combine(dateTimePath, imagePrintedPath);
            Directory.CreateDirectory(pathString);

            return (System.IO.Path.Combine(currentPath, dateTimePath), pathString); // Root Path
        }
    }

    class ReSize
    {
        static public void CropAndSaveImage(string imagepath, int photoInTemplateNumb)
        {
            int cropX = 1600;
            int cropWidth = 2850;

            using (Image image = Image.Load(imagepath))
            {
                int cropHeight = image.Height; // Giữ nguyên chiều cao
                image.Mutate(x => x.Crop(new Rectangle(cropX, 0, cropWidth, cropHeight)));
                image.Save(Naming(photoInTemplateNumb));
            }
        }

        public static string Naming(int numb)
        {
            string currentPath = Environment.CurrentDirectory;
            string dateTimePath = Actual.DateNow();

            var path = Path.Combine(currentPath, dateTimePath);
            string p2 = ("IMG_" + numb.ToString() + ".jpg");
            return System.IO.Path.Combine(path, p2);
        }
    }
}