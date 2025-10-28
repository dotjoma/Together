using System.Globalization;
using System.Windows.Data;

namespace Together.Presentation.Converters;

public class MoodToEmojiConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string mood)
        {
            return mood switch
            {
                "Happy" => "😊",
                "Excited" => "🤩",
                "Calm" => "😌",
                "Stressed" => "😰",
                "Anxious" => "😟",
                "Sad" => "😢",
                "Angry" => "😠",
                _ => "😐"
            };
        }

        return "😐";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
