using CommunityToolkit.Mvvm.ComponentModel;
using MauiAppWeatherClient.Views;
using MauiAppWeatherModel;
using MauiAppWeatherServices.Interfaces;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Globalization;
using Location = MauiAppWeatherModel.Location;

namespace MauiAppWeatherClient.ViewModels
{
   public class DailyViewModel : ObservableObject
   {
      private readonly ILocationsService _locationsService;
      private readonly IWeatherService _weatherService;
      private readonly CultureInfo _culture = new("ro-RO");

      private string _selectedCityDisplay = "Bucuresti";
      private string _selectedCityQuery = "Bucharest, RO";
      private string _forecastStatus = string.Empty;
      private string _summaryText = "Prognoza zilnica nu este disponibila.";
      private bool _isBusy;
      private bool _isNightMode = true;

      private string _detailTemperature = "-";
      private string _detailFeelsLike = "-";
      private string _detailWind = "-";
      private string _detailCondition = "-";
      private string _detailUv = "-";
      private string _detailHumidity = "-";
      private string _detailPrecipitation = "-";
      private string _detailCloudiness = "-";

      private DailyForecastItem? _selectedDay;

      public ObservableCollection<DailyForecastItem> DailyItems { get; } = new();

      public Command<DailyForecastItem> SelectDayCommand { get; }
      public Command ShowDayCommand { get; }
      public Command ShowNightCommand { get; }
      public Command OpenDetailsCommand { get; }

      public string SelectedCityDisplay
      {
         get => _selectedCityDisplay;
         set => SetProperty(ref _selectedCityDisplay, value);
      }

      public string ForecastStatus
      {
         get => _forecastStatus;
         set => SetProperty(ref _forecastStatus, value);
      }

      public string SummaryText
      {
         get => _summaryText;
         set => SetProperty(ref _summaryText, value);
      }

      public bool IsBusy
      {
         get => _isBusy;
         set => SetProperty(ref _isBusy, value);
      }

      public bool IsNightMode
      {
         get => _isNightMode;
         set
         {
            if (SetProperty(ref _isNightMode, value))
            {
               OnPropertyChanged(nameof(DayTabTextColor));
               OnPropertyChanged(nameof(NightTabTextColor));
               OnPropertyChanged(nameof(DayTabUnderlineColor));
               OnPropertyChanged(nameof(NightTabUnderlineColor));
            }
         }
      }

      public string DetailTemperature
      {
         get => _detailTemperature;
         set => SetProperty(ref _detailTemperature, value);
      }

      public string DetailFeelsLike
      {
         get => _detailFeelsLike;
         set => SetProperty(ref _detailFeelsLike, value);
      }

      public string DetailWind
      {
         get => _detailWind;
         set => SetProperty(ref _detailWind, value);
      }

      public string DetailCondition
      {
         get => _detailCondition;
         set => SetProperty(ref _detailCondition, value);
      }

      public string DetailUv
      {
         get => _detailUv;
         set => SetProperty(ref _detailUv, value);
      }

      public string DetailHumidity
      {
         get => _detailHumidity;
         set => SetProperty(ref _detailHumidity, value);
      }

      public string DetailPrecipitation
      {
         get => _detailPrecipitation;
         set => SetProperty(ref _detailPrecipitation, value);
      }

      public string DetailCloudiness
      {
         get => _detailCloudiness;
         set => SetProperty(ref _detailCloudiness, value);
      }

      public string DayTabTextColor => IsNightMode ? "#5D6F98" : "#F2F6FF";
      public string NightTabTextColor => IsNightMode ? "#F2F6FF" : "#5D6F98";
      public string DayTabUnderlineColor => IsNightMode ? "Transparent" : "#F2F6FF";
      public string NightTabUnderlineColor => IsNightMode ? "#F2F6FF" : "Transparent";
      public bool CanOpenDetails => _selectedDay?.HasData == true;

      public DailyViewModel(ILocationsService locationsService, IWeatherService weatherService)
      {
         _locationsService = locationsService;
         _weatherService = weatherService;

         SelectDayCommand = new Command<DailyForecastItem>(SelectDay);
         ShowDayCommand = new Command(() => SetTabMode(false));
         ShowNightCommand = new Command(() => SetTabMode(true));
         OpenDetailsCommand = new Command(async () => await OpenDetailsAsync());
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
            ForecastStatus = "Se incarca prognoza zilnica...";
            SummaryText = "Se pregateste sumarul zilei.";
            DailyItems.Clear();
            _selectedDay = null;
            OnPropertyChanged(nameof(CanOpenDetails));
            ResetDetails();

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

            _selectedCityQuery = cityForForecast;

            var prognosis = await _weatherService.GetHourlyWeathersAsync(cityForForecast) ?? new List<HourlyWeather>();
            var orderedPrognosis = prognosis.OrderBy(item => item.Time).ToList();

            if (!orderedPrognosis.Any())
            {
               ForecastStatus = "Nu exista prognoza zilnica momentan.";
               SummaryText = "Datele zilnice sunt indisponibile acum.";
               return;
            }

            var groupedDays = orderedPrognosis
               .GroupBy(item => item.Time.Date)
               .OrderBy(group => group.Key)
               .ToDictionary(group => group.Key, group => group.OrderBy(item => item.Time).ToList());

            var firstDate = groupedDays.Keys.Min();
            for (int dayOffset = 0; dayOffset < 7; dayOffset++)
            {
               var targetDate = firstDate.AddDays(dayOffset);
               if (groupedDays.TryGetValue(targetDate, out var points))
               {
                  var representative = SelectRepresentativePoint(points);

                  DailyItems.Add(new DailyForecastItem
                  {
                     Date = targetDate,
                     DayLabel = BuildDayLabel(targetDate),
                     DayNumber = targetDate.ToString("dd", _culture),
                     IconGlyph = MapConditionToIcon(representative.IconCode, representative.Condition),
                     MaxTemperature = $"{Math.Round(points.Max(item => item.Temperature)):0}\u00B0",
                     MinTemperature = $"{Math.Round(points.Min(item => item.Temperature)):0}\u00B0",
                     Precipitation = $"{Math.Round(points.Max(item => item.PrecipitationProbability)):0}%",
                     Condition = ToConditionText(representative.Condition),
                     Points = points,
                     HasData = true
                  });
               }
               else
               {
                  DailyItems.Add(new DailyForecastItem
                  {
                     Date = targetDate,
                     DayLabel = BuildDayLabel(targetDate),
                     DayNumber = targetDate.ToString("dd", _culture),
                     IconGlyph = "\u2014",
                     MaxTemperature = "--",
                     MinTemperature = "--",
                     Precipitation = "--",
                     Condition = "Fara date",
                     Points = new List<HourlyWeather>(),
                     HasData = false
                  });
               }
            }

            if (!DailyItems.Any())
            {
               ForecastStatus = "Nu exista prognoza zilnica momentan.";
               SummaryText = "Datele zilnice sunt indisponibile acum.";
               return;
            }

            var firstDataItem = DailyItems.FirstOrDefault(item => item.HasData);
            SelectDay(firstDataItem ?? DailyItems[0]);
            ForecastStatus = string.Empty;
         }
         catch (Exception exception)
         {
            System.Diagnostics.Debug.WriteLine($"Daily load error: {exception.Message}");
            ForecastStatus = "Prognoza zilnica este indisponibila.";
            SummaryText = "Datele zilnice sunt indisponibile acum.";
         }
         finally
         {
            IsBusy = false;
         }
      }

      private void SetTabMode(bool nightMode)
      {
         if (IsNightMode == nightMode)
         {
            return;
         }

         IsNightMode = nightMode;
         UpdateSelectedDayDetails();
      }

      private void SelectDay(DailyForecastItem? day)
      {
         if (day is null)
         {
            return;
         }

         foreach (var item in DailyItems)
         {
            item.IsSelected = item == day;
         }

         _selectedDay = day;
         OnPropertyChanged(nameof(CanOpenDetails));
         UpdateSelectedDayDetails();
      }

      private async Task OpenDetailsAsync()
      {
         if (!CanOpenDetails || _selectedDay is null)
         {
            return;
         }

         var mode = IsNightMode ? "night" : "day";
         var route = $"{nameof(DailyDetailsContentPage)}?dateTicks={_selectedDay.Date.Ticks}&city={Uri.EscapeDataString(_selectedCityQuery)}&mode={mode}";

         await Shell.Current.GoToAsync(route);
      }

      private void UpdateSelectedDayDetails()
      {
         if (_selectedDay == null || !_selectedDay.Points.Any())
         {
            ResetDetails();
            SummaryText = "Pentru aceasta zi nu exista date in raspunsul API-ului de prognoza.";
            return;
         }

         var dayPoints = _selectedDay.Points.OrderBy(item => item.Time).ToList();
         var segmentPoints = dayPoints
            .Where(item => IsNightMode
               ? item.Time.Hour < 6 || item.Time.Hour >= 18
               : item.Time.Hour >= 6 && item.Time.Hour < 18)
            .ToList();

         if (!segmentPoints.Any())
         {
            segmentPoints = dayPoints;
         }

         var dominantCondition = segmentPoints
            .GroupBy(item => item.Condition)
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key)
            .First()
            .Key;

         var averageWindSpeed = segmentPoints.Average(item => item.WindSpeed);
         var dominantDirection = SelectDominantDirection(segmentPoints.Select(item => item.WindDirectionDegrees));
         var averageCloudiness = segmentPoints.Average(item => item.Cloudiness);
         var averageHumidity = segmentPoints.Average(item => item.Humidity);
         var maxPrecipitation = segmentPoints.Max(item => item.PrecipitationProbability);
         var averageFeelsLike = segmentPoints.Average(item => item.FeelsLikeTemperature);

         var segmentTemperature = IsNightMode
            ? segmentPoints.Min(item => item.Temperature)
            : segmentPoints.Max(item => item.Temperature);

         DetailTemperature = $"{Math.Round(segmentTemperature):0}\u00B0";
         DetailFeelsLike = $"{Math.Round(averageFeelsLike):0}\u00B0";
         DetailWind = $"{Math.Round(Math.Max(0, averageWindSpeed) * 3.6):0} kph {GetDirectionText(dominantDirection)}";
         DetailCondition = ToConditionText(dominantCondition);
         DetailUv = BuildUvSummary(IsNightMode, averageCloudiness);
         DetailHumidity = $"{Math.Round(Math.Clamp(averageHumidity, 0, 100)):0}% - {HumidityLabel(averageHumidity)}";
         DetailPrecipitation = $"{Math.Round(Math.Clamp(maxPrecipitation, 0, 100)):0}%";
         DetailCloudiness = $"{Math.Round(Math.Clamp(averageCloudiness, 0, 100)):0}%";

         SummaryText = BuildSummaryText(
            _selectedDay.Date,
            dominantCondition,
            dayPoints.Min(item => item.Temperature),
            dayPoints.Max(item => item.Temperature),
            averageWindSpeed,
            dominantDirection);
      }

      private void ResetDetails()
      {
         DetailTemperature = "-";
         DetailFeelsLike = "-";
         DetailWind = "-";
         DetailCondition = "-";
         DetailUv = "-";
         DetailHumidity = "-";
         DetailPrecipitation = "-";
         DetailCloudiness = "-";
      }

      private string BuildSummaryText(
         DateTime date,
         string condition,
         double minTemperature,
         double maxTemperature,
         double windSpeedMetersPerSecond,
         int windDirectionDegrees)
      {
         var dayName = _culture.DateTimeFormat.GetAbbreviatedDayName(date.DayOfWeek).TrimEnd('.');
         var monthName = _culture.DateTimeFormat.GetAbbreviatedMonthName(date.Month).TrimEnd('.');
         var period = IsNightMode ? "Noapte" : "Zi";
         var windKph = Math.Max(0, windSpeedMetersPerSecond) * 3.6;

         var lowerWind = Math.Max(0, windKph - 6);
         var upperWind = windKph + 6;

         return $"{dayName}, {monthName}. {date:dd} - {period}. {ToConditionText(condition)}. Minima {Math.Round(minTemperature):0}\u00B0C, maxima {Math.Round(maxTemperature):0}\u00B0C. Vant {GetDirectionText(windDirectionDegrees)} intre {Math.Round(lowerWind):0} si {Math.Round(upperWind):0} km/h.";
      }

      private static int SelectDominantDirection(IEnumerable<int> directions)
      {
         var bucket = directions
            .Select(value => ((value % 360) + 360) % 360)
            .GroupBy(value => (int)Math.Round(value / 22.5, MidpointRounding.AwayFromZero) % 16)
            .OrderByDescending(group => group.Count())
            .Select(group => group.Key)
            .FirstOrDefault();

         return (int)Math.Round(bucket * 22.5, MidpointRounding.AwayFromZero) % 360;
      }

      private string BuildDayLabel(DateTime date)
      {
         var shortDay = _culture.DateTimeFormat.GetAbbreviatedDayName(date.DayOfWeek)
            .Trim()
            .ToLower(_culture);

         return shortDay.EndsWith(".")
            ? shortDay
            : $"{shortDay}.";
      }

      private static HourlyWeather SelectRepresentativePoint(IReadOnlyCollection<HourlyWeather> points)
      {
         return points
            .OrderByDescending(item => item.PrecipitationProbability)
            .ThenByDescending(item => item.Temperature)
            .First();
      }

      private static string GetDisplayCity(string cityName)
      {
         return cityName.Split(',').FirstOrDefault()?.Trim() ?? cityName;
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

      private static string ToConditionText(string condition)
      {
         return condition switch
         {
            "Clear" => "Senin",
            "Clouds" => "Partial innorat",
            "Rain" => "Ploaie",
            "Drizzle" => "Burnita",
            "Thunderstorm" => "Furtuna",
            "Snow" => "Ninsoare",
            "Mist" or "Fog" => "Ceata",
            _ => "Vreme variabila"
         };
      }

      private static string BuildUvSummary(bool isNightMode, double averageCloudiness)
      {
         if (isNightMode)
         {
            return "0 - Scazut";
         }

         var estimatedUv = Math.Clamp(8 - (averageCloudiness / 20d), 1, 11);
         var label = estimatedUv switch
         {
            < 3 => "Scazut",
            < 6 => "Moderat",
            < 8 => "Ridicat",
            < 11 => "Foarte ridicat",
            _ => "Extrem"
         };

         return $"{Math.Round(estimatedUv):0} - {label}";
      }

      private static string HumidityLabel(double humidity)
      {
         return humidity switch
         {
            < 40 => "Scazuta",
            < 70 => "Medie",
            _ => "Ridicata"
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
   }

   public class DailyForecastItem : ObservableObject
   {
      private bool _isSelected;

      public DateTime Date { get; set; }

      public string DayLabel { get; set; } = string.Empty;

      public string DayNumber { get; set; } = "00";

      public string IconGlyph { get; set; } = "\u26C5";

      public string MaxTemperature { get; set; } = "0\u00B0";

      public string MinTemperature { get; set; } = "0\u00B0";

      public string Precipitation { get; set; } = "0%";

      public string Condition { get; set; } = "Vreme variabila";

      public List<HourlyWeather> Points { get; set; } = new();

      public bool HasData { get; set; } = true;

      public bool IsSelected
      {
         get => _isSelected;
         set
         {
            if (SetProperty(ref _isSelected, value))
            {
               OnPropertyChanged(nameof(CardBackground));
               OnPropertyChanged(nameof(PrimaryTextColor));
               OnPropertyChanged(nameof(SecondaryTextColor));
            }
         }
      }

      public string CardBackground => IsSelected ? "#2C395B" : (HasData ? "Transparent" : "#0D1A36");
      public string PrimaryTextColor => HasData ? "#F3F8FF" : "#8EA0C7";
      public string SecondaryTextColor => IsSelected ? "#DEE7FF" : (HasData ? "#A9B7D4" : "#6E7F9F");
   }
}
