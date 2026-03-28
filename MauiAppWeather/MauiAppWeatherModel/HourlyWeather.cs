using SQLite;

namespace MauiAppWeatherModel
{
   public class HourlyWeather
   {
      [PrimaryKey]
      public DateTime Time { get; set; }

      public double Temperature { get; set; }

      public double FeelsLikeTemperature { get; set; }

      public double PrecipitationProbability { get; set; }

      public double WindSpeed { get; set; }

      public int WindDirectionDegrees { get; set; }

      public int Humidity { get; set; }

      public int Cloudiness { get; set; }

      public string Condition { get; set; } = "Clear";

      public string IconCode { get; set; } = "01d";
   }
}
