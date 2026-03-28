using MauiAppWeatherModel;

namespace MauiAppWeatherServices.Interfaces
{
   public interface IUserSettingsService
   {
      Task<UserSettingsDto> GetUserSettingsAsync();
      Task<int> SaveUserSettingsAsync(UserSettings userSettings);
   }
}
