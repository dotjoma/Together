using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Together.Presentation.Converters;

public class BooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isInverse = parameter?.ToString()?.Equals("Inverse", StringComparison.OrdinalIgnoreCase) ?? false;
        bool boolValue = value is bool b && b;
        
        if (isInverse)
            boolValue = !boolValue;
            
        return boolValue ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isInverse = parameter?.ToString()?.Equals("Inverse", StringComparison.OrdinalIgnoreCase) ?? false;
        bool result = value is Visibility visibility && visibility == Visibility.Visible;
        
        if (isInverse)
            result = !result;
            
        return result;
    }
}
