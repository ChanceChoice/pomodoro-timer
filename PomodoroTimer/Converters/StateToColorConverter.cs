using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using PomodoroTimer.Services;

namespace PomodoroTimer.Converters;

public class StateToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TimerState state)
        {
            return state switch
            {
                TimerState.Working => new SolidColorBrush(Color.FromRgb(233, 69, 96)),
                TimerState.ShortBreak => new SolidColorBrush(Color.FromRgb(46, 213, 115)),
                TimerState.LongBreak => new SolidColorBrush(Color.FromRgb(46, 213, 115)),
                _ => new SolidColorBrush(Color.FromRgb(238, 238, 238)),
            };
        }
        return new SolidColorBrush(Colors.White);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
