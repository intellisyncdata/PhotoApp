using QRCoder;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace DemoPhotoBooth.Common
{
    public static class Helper
    {
        public static double ToDouble(this decimal value)
        {
            double.TryParse($"{value}", out double dValue);

            return dValue;
        }

        private static byte[]? GenerateQR(string link)
        {
            if (!string.IsNullOrEmpty(link))
            {
                using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
                using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(link, QRCodeGenerator.ECCLevel.Q))
                using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
                {
                    byte[] qrCodeImage = qrCode.GetGraphic(20);
                    return qrCodeImage;
                }
            }
            return null;
        }

        public static BitmapImage GenerateQRCode(string link)
        {
            var bytes = GenerateQR(link);
            if (bytes != null && bytes.Length > 0)
            {
                BitmapImage bitmapImage = new BitmapImage();
                using (MemoryStream memoryStream = new MemoryStream(bytes))
                {
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = memoryStream;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                }
                return bitmapImage;
            }
            return null;
        }
    }
}
