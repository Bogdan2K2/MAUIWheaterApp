using SQLite;

namespace MauiAppWeatherModel
{
   public class DailyWeather
   {
      [PrimaryKey]
      public DateTime Date { get; set; }

      public double MinTemperature { get; set; }

      public double MaxTemperature { get; set; }
      public string Condition { get; set; }
    }
}