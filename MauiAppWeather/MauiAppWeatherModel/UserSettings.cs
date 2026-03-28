using SQLite;
using System.ComponentModel.DataAnnotations;

namespace MauiAppWeatherModel
{
   public class UserSettings
   {
      [PrimaryKey, AutoIncrement]
      public int Id { get; set; }

      public string Name { get; set; }

      public string Email { get; set; }

      [Required]
      public string Language { get; set; }

      [Required]
      public string TemperatureUnit { get; set; }
   }
}
