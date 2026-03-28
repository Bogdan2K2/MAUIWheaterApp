using MauiAppWeatherModel;
using MauiAppWeatherServices.Interfaces;

namespace MauiAppWeatherClient
{
   public partial class App : Application
   {
      private readonly IUserSettingsService _userSettingsService;

      public App(IUserSettingsService userSettingsService)
      {
         InitializeComponent();

         this._userSettingsService = userSettingsService;
      }

      protected override Window CreateWindow(IActivationState? activationState)
      {
         Window window = new(new ContentPage());
         InitializeAsync(window);

         return window;
      }

      private async void InitializeAsync(Window window)
      {
         UserSettingsDto? settings = await _userSettingsService.GetUserSettingsAsync();

         if (settings is not null && !string.IsNullOrWhiteSpace(settings.Language))
         {
            LocalizationResourceManager.SetCulture(settings.Language);
         }
         else
         {
            await _userSettingsService.SaveUserSettingsAsync(new UserSettings()
            {
               Email = "User@email.ro",
               Name = "User",
               Language = "en",
               TemperatureUnit = "C"
            });

            LocalizationResourceManager.SetCulture("en");
         }

         window.Page = new AppShell();
      }
   }
}