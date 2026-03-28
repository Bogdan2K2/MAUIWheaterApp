using System.Text.Json.Serialization;

namespace MauiAppWeatherModel
{
   public class WeatherCondition
   {
      [JsonPropertyName("main")]
      public string Main { get; set; } = "Clear";

      [JsonPropertyName("icon")]
      public string Icon { get; set; } = "01d";
   }
}
