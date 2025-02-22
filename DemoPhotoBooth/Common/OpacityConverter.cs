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
    public class OpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isSelected && isSelected)
            {
                return 1; // Nếu selected, opacity 100%
            }
            return 0.5; // Nếu không selected, opacity 50%
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isSelected && isSelected)
            {
                return 1; // Nếu selected, opacity 100%
            }
            return 0.5; // Nếu không selected, opacity 50%
        }
    }
}
