using System.Text.Json.Serialization;

namespace MauiAppWeatherModel
{
   public class ForecastClouds
   {
      [JsonPropertyName("all")]
      public int All { get; set; }
   }
}
