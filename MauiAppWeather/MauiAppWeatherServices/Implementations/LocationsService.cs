using MauiAppWeatherModel;
using MauiAppWeatherRepository;
using MauiAppWeatherServices.Interfaces;
using SQLite;
using Location = MauiAppWeatherModel.Location;

namespace MauiAppWeatherServices.Implementations
{
   public class LocationsService(DatabaseContext databaseContext, IHttpClientFactory httpClientFactory) :
      BaseService(databaseContext, httpClientFactory),
      ILocationsService
   {
      public async Task<List<Location>> GetLocationsAsync()
      {
         var database = await DatabaseContext.Connection;
         return await database.Table<Location>().ToListAsync();
      }

      public async Task<Location> GetLocationByIdAsync(int id)
      {
         var database = await DatabaseContext.Connection;

         return await database.Table<Location>().Where(location => location.Id == id).FirstOrDefaultAsync();
      }

      public async Task<bool> DeactivateLocationAsync(int id)
      {
         var database = await DatabaseContext.Connection;
         var locationDB = await database.Table<Location>()
             .Where(location => location.Id == id)
             .FirstOrDefaultAsync();

         if (locationDB == null)
         {
            return false;
         }

         locationDB.Active = false;
         await database.UpdateAsync(locationDB);
         return true;
      }

      public async Task<int> AddLocationAsync(LocationDto model)
      {
         var database = await DatabaseContext.Connection;
         var location = new Location
         {
            CityName = model.CityName,
            ZipCode = model.ZipCode,
            Latitude = model.Latitude,
            Longitude = model.Longitude,
            Active = true
         };
         return await database.InsertAsync(location);
      }

      public async Task<Location> GetByCoordinatesAsync(double latitude, double longitude)
      {
         SQLiteAsyncConnection database = await DatabaseContext.Connection;

         var locations = await database.Table<Location>().ToListAsync();

         return locations.FirstOrDefault(x =>
             Math.Abs(x.Latitude - latitude) < 0.0001 &&
             Math.Abs(x.Longitude - longitude) < 0.0001);
      }

      public async Task<int> UpdateAsync(Location location)
      {
         SQLiteAsyncConnection database = await DatabaseContext.Connection;

         return await database.UpdateAsync(location);
      }

      public async Task SetCurrentLocationAsync(int id)
      {
         SQLiteAsyncConnection database = await DatabaseContext.Connection;

         var locations = await database.Table<Location>().ToListAsync();

         foreach (var loc in locations)
         {
            loc.IsCurrent = loc.Id == id;
            await database.UpdateAsync(loc);
         }
      }
   }
}
