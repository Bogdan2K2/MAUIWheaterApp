using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiAppWeatherClient.Resources.Localization;
using MauiAppWeatherModel;
using MauiAppWeatherServices.Interfaces;

namespace MauiAppWeatherClient.ViewModels
{
   public partial class SettingsViewModel : ObservableObject
   {

      private readonly IUserSettingsService _settingsService;

      public List<string> Languages { get; }

      public List<string> TemperatureOptions { get; }

      public SettingsViewModel(IUserSettingsService settingsService)
      {
         this._settingsService = settingsService;
         this.Languages = LocalizationResourceManager.Languages;
         this.TemperatureOptions = ["C", "F"];

         LoadAsync();
      }

      [ObservableProperty]
      private string name, email, language, temperature;

      public bool IsCelsius
      {
         get => Temperature == "C";
         set
         {
            if (value)
            {
               Temperature = "C";
            }
         }
      }

      public bool IsFahrenheit
      {
         get => Temperature == "F";
         set
         {
            if (value)
            {
               Temperature = "F";
            }
         }
      }

      private async void LoadAsync()
      {
         UserSettingsDto? settings = await this._settingsService.GetUserSettingsAsync();
         if (settings is not null)
         {
            Name = settings.Name;
            Language = Languages.FirstOrDefault(l => l.ToLower().StartsWith(settings.Language));
            Temperature = settings.TemperatureUnit ?? "C";
            Email = settings.Email;
         }
         else
         {
            Language = "ro";
            Temperature = "C";
         }
      }

      [RelayCommand]
      private async Task SaveAsync()
      {
         UserSettings userSettings = new()
         {
            Name = this.Name,
            Email = this.Email,
            Language = this.Language.ToLower()[..2],
            TemperatureUnit = this.Temperature
         };

         int rows = await _settingsService.SaveUserSettingsAsync(userSettings);

         LocalizationResourceManager.SetCulture(userSettings.Language);

         if (DeviceInfo.Platform == DevicePlatform.WinUI)
         {
            await Shell.Current.DisplayAlertAsync("Info", AppResources.SettingsSaved, "OK");
         }
         else
         {
            await Toast.Make(AppResources.SettingsSaved).Show();
         }

         Application.Current.MainPage = new AppShell();
      }

      partial void OnTemperatureChanged(string value)
      {
         OnPropertyChanged(nameof(IsCelsius));
         OnPropertyChanged(nameof(IsFahrenheit));
      }
   }
}
