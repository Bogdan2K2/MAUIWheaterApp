using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace MauiAppWeatherModel
{
   public class ForecastWind
   {
      [JsonPropertyName("speed")]
      public double Speed { get; set; }

      [JsonPropertyName("deg")]
      public int Degree { get; set; }
   }
}
