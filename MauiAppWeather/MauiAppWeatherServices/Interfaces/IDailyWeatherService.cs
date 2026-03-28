using MauiAppWeatherModel;

namespace MauiAppWeatherServices.Interfaces
{
   public interface IDailyWeatherService
   {
      Task<List<DailyWeather>> GetDailyWeatherAsync(DateTime dateTime);
   }
}