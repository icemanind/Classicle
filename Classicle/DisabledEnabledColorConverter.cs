using System;
using System.Windows.Data;
using System.Windows.Media;

namespace Classicle
{
    [ValueConversion(typeof(bool), typeof(Brush))]
    public class DisabledEnabledColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(Brush))
                throw new InvalidOperationException("The target must be a Brush");

            if ((bool) value)
            {
                return new SolidColorBrush(Color.FromArgb(255, 191, 191, 191));
            }
            else
            {
                return new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            }
            
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
