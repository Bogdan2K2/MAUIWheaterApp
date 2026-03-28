using SQLite;

namespace MauiAppWeatherModel
{
   public class Location
   {
      [PrimaryKey, AutoIncrement]
      public int Id { get; set; }

      public string CityName { get; set; }

      public string ZipCode { get; set; }

      public double Latitude { get; set; }

      public double Longitude { get; set; }

      public bool IsCurrent { get; set; }

      public bool Active { get; set; } = true;
   }
}
