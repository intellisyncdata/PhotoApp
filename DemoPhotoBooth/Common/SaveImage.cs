using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using DemoPhotoBooth;

namespace PhotoboothWpf
{
    class SavePhoto
    {

        public int PhotoNumber { get; set; }
        public string FolderDirectory { get; set; }

        public string PhotoName { get; set; }

        public string PhotoDirectory { get; set; }


        public SavePhoto(int numb)
        {
            PhotoNumber = numb;
            FolderDirectory = CurrenFolderDirectory();
            PhotoName = ActualPhotoName();
            PhotoDirectory = GetPhotoDirectory();

        }
        public bool CheckIfExsit(string fileName)
        {
            string currFile = Path.Combine(FolderDirectory, fileName);
            return File.Exists(currFile);
        }
        public string CurrenFolderDirectory()
        {
            string currentPath = Environment.CurrentDirectory;
            string dateTimePath = Actual.DateNow();

            var path = Path.Combine(currentPath, dateTimePath);
            return path;
        }

        public string ActualPhotoName()
        {
            int number = PhotoNumber;
            string photoName = PhotoNaming(number);
            while (CheckIfExsit(photoName) == true)
            {
                number++;
                photoName = PhotoNaming(number);
            }
            return photoName;
        }
        public string PhotoNaming(int number)
        {
            string temp = "IMG_" + number + ".jpg";
            return temp;
        }
        public string GetPhotoDirectory()
        {
            string currentPath = Actual.FilePath();
            string photoName = PhotoName;
            return Path.Combine(currentPath, photoName);
        }

    }
}
