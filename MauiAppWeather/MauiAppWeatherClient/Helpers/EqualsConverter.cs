using System.Globalization;

namespace MauiAppWeatherClient.Helpers
{
   public class EqualsConverter : IValueConverter
   {
      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
         => value?.ToString() == parameter?.ToString();

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
         => (bool)value ? parameter?.ToString() : null;
   }
}
