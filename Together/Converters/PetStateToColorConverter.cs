using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Together.Domain.Enums;

namespace Together.Presentation.Converters;

public class PetStateToColorConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2 || values[0] is not PetState state)
            return Colors.LightBlue;

        var appearance = values[1] as string ?? "default";

        // Get base color from appearance
        var baseColor = GetAppearanceColor(appearance);

        // Modify based on state
        return state switch
        {
            PetState.Happy => baseColor,
            PetState.Excited => LightenColor(baseColor, 0.3f),
            PetState.Sad => DarkenColor(baseColor, 0.3f),
            PetState.Neglected => DarkenColor(baseColor, 0.5f),
            _ => baseColor
        };
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    private Color GetAppearanceColor(string appearance)
    {
        return appearance.ToLower() switch
        {
            "default" => Colors.LightBlue,
            "blue" => Colors.DodgerBlue,
            "pink" => Colors.HotPink,
            "green" => Colors.LimeGreen,
            "yellow" => Colors.Gold,
            "purple" => Colors.MediumPurple,
            "orange" => Colors.Orange,
            "rainbow" => Colors.DeepPink, // Simplified
            "galaxy" => Colors.MidnightBlue,
            "golden" => Colors.Goldenrod,
            "diamond" => Colors.LightCyan,
            _ => Colors.LightBlue
        };
    }

    private Color LightenColor(Color color, float amount)
    {
        return Color.FromRgb(
            (byte)Math.Min(255, color.R + (255 - color.R) * amount),
            (byte)Math.Min(255, color.G + (255 - color.G) * amount),
            (byte)Math.Min(255, color.B + (255 - color.B) * amount)
        );
    }

    private Color DarkenColor(Color color, float amount)
    {
        return Color.FromRgb(
            (byte)(color.R * (1 - amount)),
            (byte)(color.G * (1 - amount)),
            (byte)(color.B * (1 - amount))
        );
    }
}
