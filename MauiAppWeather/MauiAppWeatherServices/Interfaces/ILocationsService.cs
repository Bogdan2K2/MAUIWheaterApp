using MauiAppWeatherModel;

namespace MauiAppWeatherServices.Interfaces
{
   public interface ILocationsService
   {
      Task<int> AddLocationAsync(LocationDto model);
      Task<bool> DeactivateLocationAsync(int id);
      Task<MauiAppWeatherModel.Location> GetByCoordinatesAsync(double latitude, double longitude);
      Task<MauiAppWeatherModel.Location> GetLocationByIdAsync(int id);
      Task<List<MauiAppWeatherModel.Location>> GetLocationsAsync();
      Task SetCurrentLocationAsync(int id);
      Task<int> UpdateAsync(MauiAppWeatherModel.Location location);
   }
}