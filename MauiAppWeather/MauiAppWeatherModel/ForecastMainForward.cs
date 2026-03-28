using System.Text.Json.Serialization;

namespace MauiAppWeatherModel
{
   public class ForecastMainForward
   {
      [JsonPropertyName("temp")]
      public float Temperature { get; set; }

      [JsonPropertyName("feels_like")]
      public float FeelsLike { get; set; }

      [JsonPropertyName("temp_min")]
      public float TempMin { get; set; }

      [JsonPropertyName("temp_max")]
      public float TempMax { get; set; }

      [JsonPropertyName("humidity")]
      public int Humidity { get; set; }
   }
}
