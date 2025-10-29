using System.Globalization;
using System.Windows.Data;

namespace Together.Presentation.Converters;

public class StringEqualityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
            return false;

        return value.ToString() == parameter.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isEqual && isEqual && parameter != null)
            return parameter.ToString() ?? string.Empty;

        return string.Empty;
    }
}
