using CommunityToolkit.Maui;
using MauiAppWeatherClient.ViewModels;
using MauiAppWeatherClient.Views;
using MauiAppWeatherRepository;
using MauiAppWeatherServices.Implementations;
using MauiAppWeatherServices.Interfaces;
using Microcharts.Maui;
using Microsoft.Extensions.Logging;

namespace MauiAppWeatherClient
{
   public static class MauiProgram
   {
      public static MauiApp CreateMauiApp()
      {
         var builder = MauiApp.CreateBuilder();
         builder
             .UseMauiApp<App>()
             .UseMauiCommunityToolkit(options => options.SetShouldEnableSnackbarOnWindows(true))
             .UseMicrocharts()
#if ANDROID
             .UseMauiMaps()
#endif
             .ConfigureFonts(fonts =>
             {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
             });

#if DEBUG
         builder.Logging.AddDebug();
#endif
         builder.Services.AddHttpClient();
         builder.Services.AddSingleton<DatabaseContext>();
         builder.Services.AddScoped<ILocationsService, LocationsService>();
         builder.Services.AddScoped<IDailyWeatherService, DailyWeatherService>();
         builder.Services.AddScoped<IHourlyWeatherService, HourlyWeatherService>();
         builder.Services.AddScoped<IUserSettingsService, UserSettingsService>();
         builder.Services.AddScoped<IWeatherService, WeatherService>();

         builder.Services.AddTransient<HomePageViewModel>();
         builder.Services.AddTransient<HourlyViewModel>();
         builder.Services.AddTransient<DailyViewModel>();
         builder.Services.AddTransient<DailyDetailsViewModel>();
         builder.Services.AddTransient<SettingsViewModel>();
         builder.Services.AddTransient<MapViewModel>();
         builder.Services.AddTransient<NewLocationViewModel>();

         builder.Services.AddTransient<HomePage>();
         builder.Services.AddTransient<HourlyPage>();
         builder.Services.AddTransient<DailyPage>();
         builder.Services.AddTransient<DailyDetailsContentPage>();
         builder.Services.AddTransient<NewLocationPage>();

         return builder.Build();
      }
   }
}
