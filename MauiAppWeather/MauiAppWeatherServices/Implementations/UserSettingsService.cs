using MauiAppWeatherModel;
using MauiAppWeatherRepository;
using MauiAppWeatherServices.Interfaces;
using SQLite;

namespace MauiAppWeatherServices.Implementations
{
   public class UserSettingsService(DatabaseContext databaseContext, IHttpClientFactory httpClientFactory) :
      BaseService(databaseContext, httpClientFactory),
      IUserSettingsService
   {
      public async Task<UserSettingsDto> GetUserSettingsAsync()
      {
         SQLiteAsyncConnection connection = await DatabaseContext.Connection;

         UserSettings userSettings =
             await connection.Table<UserSettings>().FirstOrDefaultAsync();

         if (userSettings is null)
         {
            return null;
         }

         return new UserSettingsDto
         {
            Id = userSettings.Id,
            Email = userSettings.Email,
            Name = userSettings.Name,
            Language = userSettings.Language,
            TemperatureUnit = userSettings.TemperatureUnit
         };
      }

      public async Task<int> SaveUserSettingsAsync(UserSettings userSettings)
      {
         SQLiteAsyncConnection connection = await this.DatabaseContext.Connection;
         UserSettings currentUserSettings = await connection.Table<UserSettings>().FirstOrDefaultAsync();
         if (currentUserSettings is null)
         {
            return await connection.InsertAsync(userSettings);
         }
         else
         {
            currentUserSettings = new()
            {
               Id = currentUserSettings.Id,
               Name = userSettings.Name,
               Email = userSettings.Email,
               Language = userSettings.Language,
               TemperatureUnit = userSettings.TemperatureUnit
            };

            return await connection.UpdateAsync(currentUserSettings);
         }
      }
   }
}
