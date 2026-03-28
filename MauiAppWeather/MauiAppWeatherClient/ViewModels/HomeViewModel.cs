using CommunityToolkit.Mvvm.ComponentModel;
using MauiAppWeatherModel;
using MauiAppWeatherServices.Interfaces;
using Microcharts;
using SkiaSharp;
using System.Collections.ObjectModel;
using Location = MauiAppWeatherModel.Location;

namespace MauiAppWeatherClient.ViewModels
{
   public partial class HomePageViewModel : ObservableObject
   {
      private readonly ILocationsService _locationsServices;
      private readonly IWeatherService _weatherService;

      // Observable properties
      [ObservableProperty]
      private ObservableCollection<Location> locations;

      [ObservableProperty]
      private Location selectedLocation;

      [ObservableProperty]
      private List<HourlyWeather> hourlyWeather;

      [ObservableProperty]
      private string outlookDescription;

      [ObservableProperty]
      private Chart grafic;

      [ObservableProperty]
      private string smartAlertsText = "Nicio alerta meteo urgenta in urmatoarele ore.";

      [ObservableProperty]
      private string smartAlertsColor = "#6FD9A8";

      public HomePageViewModel(
          ILocationsService locationsServices,
          IWeatherService weatherService)
      {
         _locationsServices = locationsServices;
         _weatherService = weatherService;

         _ = LoadLocationsAsync();
      }

      // Triggered automatically when SelectedLocation changes
      partial void OnSelectedLocationChanged(Location value)
      {
         _ = HandleSelectionChanged(value);
      }

      public async Task LoadLocationsAsync()
      {
         var dbList = await _locationsServices.GetLocationsAsync()
                      ?? new List<Location>();

         var activeLocations = dbList
             .Where(l => l.Active)
             .ToList();

         activeLocations.Add(new Location
         {
            Id = -1,
            CityName = "New location...",
            Active = false
         });

         Locations = new ObservableCollection<Location>(activeLocations);

         var currentLocation = activeLocations.FirstOrDefault(l => l.IsCurrent);

         if (currentLocation != null)
         {
            SelectedLocation = currentLocation;
         }
         else if (activeLocations.Any(l => l.Id != -1))
         {
            SelectedLocation = activeLocations.First(l => l.Id != -1);
         }
      }

      private async Task HandleSelectionChanged(Location selectedLocation)
      {
         if (selectedLocation == null)
            return;

         // Navigate to new location page
         if (selectedLocation.Id == -1)
         {
            SelectedLocation = null;
            await Shell.Current.GoToAsync(nameof(Views.NewLocationPage));
            return;
         }

            try
            {
                await _locationsServices.SetCurrentLocationAsync(selectedLocation.Id);

                foreach (var loc in Locations.Where(l => l.Id != -1))
                {
                    loc.IsCurrent = (loc.Id == selectedLocation.Id);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating IsCurrent: {ex.Message}");
            }

         try
         {
            HourlyWeather = new List<HourlyWeather>();

            var prognosis = await _weatherService
                .GetHourlyWeathersAsync(selectedLocation.CityName);

            UpdateSmartAlerts(prognosis ?? new List<HourlyWeather>());

            if (prognosis != null && prognosis.Any())
            {
               var sixHourList = new List<HourlyWeather>();

               DateTime startTime = new DateTime(
                   DateTime.Now.Year,
                   DateTime.Now.Month,
                   DateTime.Now.Day,
                   DateTime.Now.Hour,
                   0,
                   0);

               for (int i = 0; i < 6; i++)
               {
                  DateTime targetTime = startTime.AddHours(i);

                  var before = prognosis.LastOrDefault(p => p.Time <= targetTime)
                               ?? prognosis.First();

                  var after = prognosis.FirstOrDefault(p => p.Time >= targetTime)
                              ?? prognosis.Last();

                  float estimatedTemp;

                  if (before.Time == after.Time)
                  {
                     estimatedTemp = (float)before.Temperature;
                  }
                  else
                  {
                     double factor =
                         (targetTime - before.Time).TotalMinutes /
                         (after.Time - before.Time).TotalMinutes;

                     estimatedTemp = (float)(
                         before.Temperature +
                         (after.Temperature - before.Temperature) * factor);
                  }

                  sixHourList.Add(new HourlyWeather
                  {
                     Time = targetTime,
                     Temperature = estimatedTemp
                  });
               }

               HourlyWeather = sixHourList;

               var chartEntries = sixHourList.Select(w =>
                   new ChartEntry((float)w.Temperature)
                   {
                      Label = w.Time.ToString("HH:mm"),
                      ValueLabel = $"{Math.Round(w.Temperature)}°",
                      Color = SKColor.Parse("#87CEEB")
                   }).ToList();

               Grafic = new LineChart
               {
                  Entries = chartEntries,
                  LineMode = LineMode.Straight,
                  PointMode = PointMode.Circle,
                  LabelTextSize = 24f,
                  BackgroundColor = SKColors.AliceBlue,
                  LabelOrientation = Orientation.Horizontal,
                  ValueLabelOrientation = Orientation.Horizontal
               };
            }
            else
            {
               OutlookDescription = "Nu exista date meteo suficiente pentru aceasta locatie.";
            }

            var dailyInfo = await _weatherService
                .GetDailyWeatherAsync(selectedLocation.CityName);

            if (dailyInfo != null)
            {
               string condition = dailyInfo.Condition switch
               {
                  "Clear" => "Clear skies with light wind",
                  "Clouds" => "Partly cloudy",
                  "Rain" or "Drizzle" => "Precipitation expected",
                  "Snow" => "Snowy conditions",
                  "Thunderstorm" => "Stormy weather",
                  _ => "Variable weather"
               };

               string minTemp = Math.Round(dailyInfo.MinTemperature).ToString();
               string maxTemp = Math.Round(dailyInfo.MaxTemperature).ToString();

                    OutlookDescription = $"{condition}. Temperatures would be between " +
                                         $"Min: {minTemp}°C and Max: {maxTemp}°C through the day.";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating UI: {ex.Message}");
            }
        }

      private void UpdateSmartAlerts(List<HourlyWeather> forecast)
      {
         var alerts = new List<string>();
         var now = DateTime.Now;
         var next12Hours = forecast
            .Where(item => item.Time >= now && item.Time <= now.AddHours(12))
            .OrderBy(item => item.Time)
            .ToList();

         var rainPoint = next12Hours.FirstOrDefault(item =>
            IsRainCondition(item.Condition) || item.PrecipitationProbability >= 60);

         if (rainPoint != null)
         {
            var minutesUntilRain = Math.Max(0, (int)Math.Round((rainPoint.Time - now).TotalMinutes));
            var roundedMinutes = RoundToNearest(minutesUntilRain, 15);
            alerts.Add($"Ploaie posibila in ~{FormatMinutes(roundedMinutes)}.");
         }

         var strongWindPoint = next12Hours
            .OrderByDescending(item => item.WindSpeed)
            .FirstOrDefault(item => item.WindSpeed * 3.6 >= 40);

         if (strongWindPoint != null)
         {
            alerts.Add($"Vant puternic: pana la {Math.Round(strongWindPoint.WindSpeed * 3.6):0} km/h.");
         }

         var nextNightPoints = forecast
            .Where(item =>
               item.Time >= now &&
               item.Time <= now.AddHours(24) &&
               (item.Time.Hour >= 21 || item.Time.Hour < 6))
            .ToList();

         if (nextNightPoints.Any())
         {
            var minimumNightTemp = nextNightPoints.Min(item => item.Temperature);
            if (minimumNightTemp <= 0)
            {
               alerts.Add($"Risc de inghet la noapte (min {Math.Round(minimumNightTemp):0}°C).");
            }
         }

         if (alerts.Any())
         {
            SmartAlertsText = string.Join("\n", alerts.Take(3));
            SmartAlertsColor = "#FFBE5C";
            return;
         }

         SmartAlertsText = "Nicio alerta meteo urgenta in urmatoarele ore.";
         SmartAlertsColor = "#6FD9A8";
      }

      private static bool IsRainCondition(string condition)
      {
         return condition is "Rain" or "Drizzle" or "Thunderstorm" or "Snow";
      }

      private static int RoundToNearest(int value, int step)
      {
         if (step <= 0)
         {
            return value;
         }

         return (int)(Math.Round(value / (double)step, MidpointRounding.AwayFromZero) * step);
      }

      private static string FormatMinutes(int minutes)
      {
         if (minutes < 60)
         {
            return $"{minutes} min";
         }

         var hours = minutes / 60;
         var mins = minutes % 60;
         return mins == 0 ? $"{hours}h" : $"{hours}h {mins}m";
      }
   }
}
