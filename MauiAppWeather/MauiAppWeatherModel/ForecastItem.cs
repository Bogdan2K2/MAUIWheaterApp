using System.Text.Json.Serialization;

namespace MauiAppWeatherModel
{
   public class ForecastItem
   {
      [JsonPropertyName("main")]
      public ForecastMainForward Main { get; set; } = new();

      [JsonPropertyName("dt_txt")]
      public string DataText { get; set; } = string.Empty;

      [JsonPropertyName("weather")]
      public List<WeatherCondition> Weather { get; set; } = new List<WeatherCondition>();

      [JsonPropertyName("pop")]
      public double PrecipitationProbability { get; set; }

      [JsonPropertyName("wind")]
      public ForecastWind Wind { get; set; } = new();

      [JsonPropertyName("clouds")]
      public ForecastClouds Clouds { get; set; } = new();
   }
}
