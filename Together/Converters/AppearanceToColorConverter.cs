using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Together.Presentation.Converters;

public class AppearanceToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var appearance = value as string ?? "default";

        return appearance.ToLower() switch
        {
            "default" => Colors.LightBlue,
            "blue" => Colors.DodgerBlue,
            "pink" => Colors.HotPink,
            "green" => Colors.LimeGreen,
            "yellow" => Colors.Gold,
            "purple" => Colors.MediumPurple,
            "orange" => Colors.Orange,
            "rainbow" => Colors.DeepPink,
            "galaxy" => Colors.MidnightBlue,
            "golden" => Colors.Goldenrod,
            "diamond" => Colors.LightCyan,
            _ => Colors.LightBlue
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
