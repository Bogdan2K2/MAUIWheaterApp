using CommunityToolkit.Mvvm.ComponentModel;
using MauiAppWeatherModel;
using MauiAppWeatherServices.Interfaces;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Globalization;
using Location = MauiAppWeatherModel.Location;

namespace MauiAppWeatherClient.ViewModels
{
   public class HourlyViewModel : ObservableObject
   {
      private readonly ILocationsService _locationsService;
      private readonly IWeatherService _weatherService;

      private string _selectedCityDisplay = "Bucuresti";
      private string _currentDateLabel = string.Empty;
      private string _forecastStatus = string.Empty;
      private string _daySummary = "No day summary available.";
      private bool _isBusy;

      public ObservableCollection<HourlyForecastItem> HourlyItems { get; } = new ObservableCollection<HourlyForecastItem>();
      public Command<HourlyForecastItem> ToggleDescriptionCommand { get; }

      public string SelectedCityDisplay
      {
         get => _selectedCityDisplay;
         set => SetProperty(ref _selectedCityDisplay, value);
      }

      public string CurrentDateLabel
      {
         get => _currentDateLabel;
         set => SetProperty(ref _currentDateLabel, value);
      }

      public string ForecastStatus
      {
         get => _forecastStatus;
         set => SetProperty(ref _forecastStatus, value);
      }

      public string DaySummary
      {
         get => _daySummary;
         set => SetProperty(ref _daySummary, value);
      }

      public bool IsBusy
      {
         get => _isBusy;
         set => SetProperty(ref _isBusy, value);
      }

      public HourlyViewModel(ILocationsService locationsService, IWeatherService weatherService)
      {
         _locationsService = locationsService;
         _weatherService = weatherService;
         ToggleDescriptionCommand = new Command<HourlyForecastItem>(ToggleDescription);
         CurrentDateLabel = BuildDateLabel(DateTime.Now);
      }

      public async Task LoadAsync()
      {
         if (IsBusy)
         {
            return;
         }

         IsBusy = true;

         try
         {
            CurrentDateLabel = BuildDateLabel(DateTime.Now);
            ForecastStatus = "Loading hourly forecast...";
            DaySummary = "Preparing day summary...";
            HourlyItems.Clear();

            var locations = await _locationsService.GetLocationsAsync() ?? new List<Location>();
            var activeLocations = locations.Where(location => location.Active).ToList();

            var selectedLocation = activeLocations.FirstOrDefault(location => location.IsCurrent)
               ?? activeLocations.FirstOrDefault();

            string cityForForecast;
            if (selectedLocation == null)
            {
               SelectedCityDisplay = "Bucuresti";
               cityForForecast = "Bucharest, RO";
            }
            else
            {
               SelectedCityDisplay = GetDisplayCity(selectedLocation.CityName);
               cityForForecast = selectedLocation.CityName;
               await _locationsService.SetCurrentLocationAsync(selectedLocation.Id);
            }

            var prognosis = await _weatherService.GetHourlyWeathersAsync(cityForForecast) ?? new List<HourlyWeather>();
            var orderedPrognosis = prognosis.OrderBy(item => item.Time).ToList();

            if (!orderedPrognosis.Any())
            {
               ForecastStatus = "No hourly forecast available right now.";
               DaySummary = "No day summary available.";
               return;
            }

            DaySummary = BuildDaySummary(orderedPrognosis);

            DateTime startTime = new DateTime(
               DateTime.Now.Year,
               DateTime.Now.Month,
               DateTime.Now.Day,
               DateTime.Now.Hour,
               0,
               0);

            for (int index = 0; index < 24; index++)
            {
               DateTime targetTime = startTime.AddHours(index);
               var pointBefore = orderedPrognosis.LastOrDefault(item => item.Time <= targetTime) ?? orderedPrognosis.First();
               var pointAfter = orderedPrognosis.FirstOrDefault(item => item.Time >= targetTime) ?? orderedPrognosis.Last();

               double interpolationFactor = 0;
               if (pointBefore.Time != pointAfter.Time)
               {
                  interpolationFactor = (targetTime - pointBefore.Time).TotalMinutes
                     / (pointAfter.Time - pointBefore.Time).TotalMinutes;
               }

               double temperature = Interpolate(pointBefore.Temperature, pointAfter.Temperature, interpolationFactor);
               double precipitation = Interpolate(pointBefore.PrecipitationProbability, pointAfter.PrecipitationProbability, interpolationFactor);
               double windSpeed = Interpolate(pointBefore.WindSpeed, pointAfter.WindSpeed, interpolationFactor);
               HourlyWeather iconSource = SelectNearestPoint(pointBefore, pointAfter, targetTime);

               HourlyItems.Add(new HourlyForecastItem
               {
                  Hour = targetTime.ToString("HH"),
                  ConditionIcon = MapConditionToIcon(iconSource.IconCode, iconSource.Condition),
                  Precipitation = $"{Math.Round(Math.Clamp(precipitation, 0, 100)):0}%",
                  Temperature = $"{Math.Round(temperature):0}\u00B0",
                  Wind = $"{Math.Round(Math.Max(0, windSpeed) * 3.6):0} kph {GetDirectionText(iconSource.WindDirectionDegrees)}",
                  ShortDescription = BuildShortDescription(
                     iconSource.Condition,
                     temperature,
                     precipitation,
                     windSpeed,
                     iconSource.WindDirectionDegrees)
               });
            }

            ForecastStatus = string.Empty;
         }
         catch (Exception exception)
         {
            System.Diagnostics.Debug.WriteLine($"Hourly load error: {exception.Message}");
            ForecastStatus = "Hourly forecast is unavailable.";
            DaySummary = "No day summary available.";
         }
         finally
         {
            IsBusy = false;
         }
      }

      private static HourlyWeather SelectNearestPoint(HourlyWeather before, HourlyWeather after, DateTime targetTime)
      {
         var minutesFromBefore = Math.Abs((targetTime - before.Time).TotalMinutes);
         var minutesToAfter = Math.Abs((after.Time - targetTime).TotalMinutes);
         return minutesFromBefore <= minutesToAfter ? before : after;
      }

      private static double Interpolate(double start, double end, double factor)
      {
         return start + ((end - start) * factor);
      }

      private static string GetDisplayCity(string cityName)
      {
         return cityName.Split(',').FirstOrDefault()?.Trim() ?? cityName;
      }

      private static string BuildDateLabel(DateTime dateTime)
      {
         var culture = new CultureInfo("ro-RO");
         var dayName = culture.DateTimeFormat.GetDayName(dateTime.DayOfWeek);

         var monthAbbreviation = culture.DateTimeFormat.GetAbbreviatedMonthName(dateTime.Month).TrimEnd('.');
         monthAbbreviation = monthAbbreviation.Length > 3
            ? monthAbbreviation[..3]
            : monthAbbreviation;

         return $"{dayName}, {monthAbbreviation}. {dateTime:dd}";
      }

      private static string BuildDaySummary(IReadOnlyCollection<HourlyWeather> prognosis)
      {
         if (prognosis.Count == 0)
         {
            return "No day summary available.";
         }

         var today = DateTime.Now.Date;
         var dayItems = prognosis
            .Where(item => item.Time.Date == today)
            .ToList();

         if (!dayItems.Any())
         {
            dayItems = prognosis
               .OrderBy(item => item.Time)
               .Take(24)
               .ToList();
         }

         double minTemperature = dayItems.Min(item => item.Temperature);
         double maxTemperature = dayItems.Max(item => item.Temperature);
         double maxPrecipitation = dayItems.Max(item => item.PrecipitationProbability);

         string primaryCondition = dayItems
            .Select(item => ToSummaryCondition(item.Condition))
            .GroupBy(condition => condition)
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key)
            .First()
            .Key;

         string precipitationSummary = maxPrecipitation switch
         {
            >= 60 => "rain likely",
            >= 30 => "some rain chances",
            _ => "mostly dry"
         };

         return $"{primaryCondition}, {Math.Round(minTemperature):0}\u00B0-{Math.Round(maxTemperature):0}\u00B0, {precipitationSummary}.";
      }

      private static string ToSummaryCondition(string condition)
      {
         return condition switch
         {
            "Clear" => "Clear skies",
            "Clouds" => "Cloudy",
            "Rain" => "Rainy",
            "Drizzle" => "Drizzle",
            "Thunderstorm" => "Stormy",
            "Snow" => "Snow possible",
            "Mist" or "Fog" => "Misty",
            _ => "Variable weather"
         };
      }

      private static string MapConditionToIcon(string iconCode, string condition)
      {
         if (!string.IsNullOrWhiteSpace(iconCode))
         {
            return iconCode switch
            {
               "01d" => "\u2600",
               "01n" => "\u263E",
               "02d" or "03d" => "\u26C5",
               "02n" or "03n" => "\u2601",
               "04d" or "04n" => "\u2601",
               "09d" or "09n" or "10d" or "10n" => "\u2602",
               "11d" or "11n" => "\u26C8",
               "13d" or "13n" => "\u2744",
               "50d" or "50n" => "MIST",
               _ => "\u26C5"
            };
         }

         return condition switch
         {
            "Clear" => "\u2600",
            "Rain" or "Drizzle" => "\u2602",
            "Thunderstorm" => "\u26C8",
            "Snow" => "\u2744",
            _ => "\u26C5"
         };
      }

      private static string GetDirectionText(int degrees)
      {
         string[] directions =
         {
            "N", "NNE", "NE", "ENE",
            "E", "ESE", "SE", "SSE",
            "S", "SSW", "SW", "WSW",
            "W", "WNW", "NW", "NNW"
         };

         var normalized = ((degrees % 360) + 360) % 360;
         var index = (int)Math.Round(normalized / 22.5, MidpointRounding.AwayFromZero) % 16;
         return directions[index];
      }

      private static string BuildShortDescription(
         string condition,
         double temperature,
         double precipitation,
         double windSpeed,
         int windDirectionDegrees)
      {
         string conditionText = condition switch
         {
            "Clear" => "Cer senin",
            "Clouds" => "Nori dispersati",
            "Rain" => "Ploaie usoara",
            "Drizzle" => "Burnita",
            "Thunderstorm" => "Posibile descarcari electrice",
            "Snow" => "Ninsoare",
            "Mist" or "Fog" => "Vizibilitate redusa",
            _ => "Vreme variabila"
         };

         string temperatureText = $"{Math.Round(temperature):0}\u00B0C";
         string precipitationText = $"{Math.Round(Math.Clamp(precipitation, 0, 100)):0}%";
         string windText = $"{Math.Round(Math.Max(0, windSpeed) * 3.6):0} kph {GetDirectionText(windDirectionDegrees)}";

         return $"{conditionText}. Temperatura estimata {temperatureText}, sansa de precipitatii {precipitationText}, vant {windText}.";
      }

      private void ToggleDescription(HourlyForecastItem? item)
      {
         if (item is null)
         {
            return;
         }

         item.IsExpanded = !item.IsExpanded;
      }
   }

   public class HourlyForecastItem : ObservableObject
   {
      private string _hour = string.Empty;
      private string _conditionIcon = "\u26C5";
      private string _precipitation = "0%";
      private string _temperature = "0\u00B0";
      private string _wind = "0 kph N";
      private string _shortDescription = string.Empty;
      private bool _isExpanded;

      public string Hour
      {
         get => _hour;
         set => SetProperty(ref _hour, value);
      }

      public string ConditionIcon
      {
         get => _conditionIcon;
         set => SetProperty(ref _conditionIcon, value);
      }

      public string Precipitation
      {
         get => _precipitation;
         set => SetProperty(ref _precipitation, value);
      }

      public string Temperature
      {
         get => _temperature;
         set => SetProperty(ref _temperature, value);
      }

      public string Wind
      {
         get => _wind;
         set => SetProperty(ref _wind, value);
      }

      public string ShortDescription
      {
         get => _shortDescription;
         set => SetProperty(ref _shortDescription, value);
      }

      public bool IsExpanded
      {
         get => _isExpanded;
         set
         {
            if (SetProperty(ref _isExpanded, value))
            {
               OnPropertyChanged(nameof(ExpandGlyph));
            }
         }
      }

      public string ExpandGlyph => IsExpanded ? "-" : "+";
   }
}
