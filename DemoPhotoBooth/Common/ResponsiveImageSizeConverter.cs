using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace DemoPhotoBooth.Common
{
    public class ResponsiveImageSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double containerWidth)
            {
                // Tính chiều rộng ảnh = 1/3 containerWidth - khoảng cách margin
                return (containerWidth / 3) - 200; // Trừ đi margin giữa các ảnh
            }
            return 200; // Giá trị mặc định
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ResponsiveImageHeightConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // Lấy thông tin từ BindingContext hoặc từ ViewModel
            if (values.Length == 2 && values[0] is double containerWidth && values[1] is string frameType)
            {
                // Tính chiều rộng của ảnh theo tỷ lệ 1/3 containerWidth - margin
                double imageWidth = (containerWidth / 3) - 100;

                // Kiểm tra FrameType và áp dụng tỷ lệ tương ứng
                double aspectRatio;
                if (frameType == "Horizontal")
                {
                    aspectRatio = 4.0 / 6.0; // ảnh dọc
                }
                else
                {
                    aspectRatio = 6.0 / 4.0; // ảnh ngang
                }

                // Tính chiều cao dựa trên chiều rộng và tỷ lệ ảnh
                return imageWidth / aspectRatio;
            }

            return 150; // Giá trị mặc định nếu không có giá trị đầu vào hợp lệ
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PlaceholderVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString() == "placeholder" ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ImageVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString() == "placeholder" ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
