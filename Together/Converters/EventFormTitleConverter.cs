using System;
using System.Globalization;
using System.Windows.Data;

namespace Together.Presentation.Converters;

public class EventFormTitleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isEditMode)
        {
            return isEditMode ? "Edit Event" : "Create Event";
        }
        return "Create Event";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
