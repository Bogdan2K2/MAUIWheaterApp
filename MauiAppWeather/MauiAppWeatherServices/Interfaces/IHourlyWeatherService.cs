using MauiAppWeatherModel;

namespace MauiAppWeatherServices.Interfaces
{
   public interface IHourlyWeatherService
   {
      Task<List<HourlyWeather>> GetHourlyWeatherAsync(DateTime dateTime);
   }
}