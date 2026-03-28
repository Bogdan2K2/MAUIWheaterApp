using System.Text.Json.Serialization;

namespace MauiAppWeatherModel
{
   public class WeatherResponseForecast
   {
      [JsonPropertyName("list")]
      public List<ForecastItem> List { get; set; } = new List<ForecastItem>();
   }
}
