using MauiAppWeatherModel;

namespace MauiAppWeatherServices.Interfaces
{
   public interface IWeatherService
   {
      Task<List<LocationDto>> SearchAsync(string query);
        Task<List<HourlyWeather>> GetHourlyWeathersAsync(string nameCity);
        Task<DailyWeather> GetDailyWeatherAsync(string cityName);


   }
}
