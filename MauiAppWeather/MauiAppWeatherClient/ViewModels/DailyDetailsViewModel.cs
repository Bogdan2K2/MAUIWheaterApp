using CommunityToolkit.Mvvm.ComponentModel;
using MauiAppWeatherModel;
using MauiAppWeatherServices.Interfaces;
using Microsoft.Maui.Controls;
using System.Globalization;

namespace MauiAppWeatherClient.ViewModels;

public class DailyDetailsViewModel : ObservableObject, IQueryAttributable
{
   private readonly IWeatherService _weatherService;
   private readonly CultureInfo _culture = new("ro-RO");

   private string _cityQuery = "Bucharest, RO";
   private DateTime _selectedDate = DateTime.Today;
   private bool _isNightMode;
   private bool _isBusy;

   private string _cityDisplay = "Bucuresti";
   private string _dayNameText = string.Empty;
   private string _dateText = string.Empty;
   private string _statusText = string.Empty;
   private string _narrativeText = "Se pregatesc detaliile meteo.";
   private string _conditionText = "-";
   private string _temperatureText = "-";
   private string _windText = "-";
   private string _humidityText = "-";
   private string _precipitationText = "-";
   private string _cloudinessText = "-";

   public Command BackCommand { get; }

   public string CityDisplay
   {
      get => _cityDisplay;
      set => SetProperty(ref _cityDisplay, value);
   }

   public string DayNameText
   {
      get => _dayNameText;
      set => SetProperty(ref _dayNameText, value);
   }

   public string DateText
   {
      get => _dateText;
      set => SetProperty(ref _dateText, value);
   }

   public string StatusText
   {
      get => _statusText;
      set => SetProperty(ref _statusText, value);
   }

   public string NarrativeText
   {
      get => _narrativeText;
      set => SetProperty(ref _narrativeText, value);
   }

   public string ConditionText
   {
      get => _conditionText;
      set => SetProperty(ref _conditionText, value);
   }

   public string TemperatureText
   {
      get => _temperatureText;
      set => SetProperty(ref _temperatureText, value);
   }

   public string WindText
   {
      get => _windText;
      set => SetProperty(ref _windText, value);
   }

   public string HumidityText
   {
      get => _humidityText;
      set => SetProperty(ref _humidityText, value);
   }

   public string PrecipitationText
   {
      get => _precipitationText;
      set => SetProperty(ref _precipitationText, value);
   }

   public string CloudinessText
   {
      get => _cloudinessText;
      set => SetProperty(ref _cloudinessText, value);
   }

   public bool IsBusy
   {
      get => _isBusy;
      set => SetProperty(ref _isBusy, value);
   }

   public DailyDetailsViewModel(IWeatherService weatherService)
   {
      _weatherService = weatherService;
      BackCommand = new Command(async () => await GoBackAsync());
      UpdateDateLabels();
   }

   public void ApplyQueryAttributes(IDictionary<string, object> query)
   {
      var dateTicksValue = ReadQueryValue(query, "dateTicks");
      if (long.TryParse(dateTicksValue, out var dateTicks))
      {
         _selectedDate = new DateTime(dateTicks);
      }

      var cityValue = ReadQueryValue(query, "city");
      if (!string.IsNullOrWhiteSpace(cityValue))
      {
         _cityQuery = Uri.UnescapeDataString(cityValue);
      }

      var modeValue = ReadQueryValue(query, "mode");
      _isNightMode = string.Equals(modeValue, "night", StringComparison.OrdinalIgnoreCase);

      CityDisplay = ExtractCityDisplay(_cityQuery);
      UpdateDateLabels();
      TriggerLoad();
   }

   private async void TriggerLoad()
   {
      await LoadAsync();
   }

   public async Task LoadAsync()
   {
      if (IsBusy)
      {
         return;
      }

      IsBusy = true;
      StatusText = "Se incarca detaliile din API...";
      NarrativeText = "Se construieste explicatia pentru ziua selectata.";
      ResetMetricValues();

      try
      {
         var points = await _weatherService.GetHourlyWeathersAsync(_cityQuery) ?? new List<HourlyWeather>();
         var dayPoints = points
            .Where(item => item.Time.Date == _selectedDate.Date)
            .OrderBy(item => item.Time)
            .ToList();

         if (!dayPoints.Any())
         {
            StatusText = "Nu exista puncte meteo pentru ziua selectata.";
            NarrativeText = $"{DayNameText}, {DateText}: API-ul nu a returnat date pentru aceasta zi.";
            return;
         }

         var segmentPoints = dayPoints
            .Where(item => _isNightMode
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

         var minTemperature = dayPoints.Min(item => item.Temperature);
         var maxTemperature = dayPoints.Max(item => item.Temperature);
         var averageFeelsLike = segmentPoints.Average(item => item.FeelsLikeTemperature);
         var averageWindSpeed = segmentPoints.Average(item => item.WindSpeed);
         var dominantDirection = SelectDominantDirection(segmentPoints.Select(item => item.WindDirectionDegrees));
         var averageHumidity = segmentPoints.Average(item => item.Humidity);
         var maxPrecipitation = segmentPoints.Max(item => item.PrecipitationProbability);
         var averageCloudiness = segmentPoints.Average(item => item.Cloudiness);

         ConditionText = ToConditionText(dominantCondition);
         TemperatureText = $"{Math.Round(minTemperature):0}°C - {Math.Round(maxTemperature):0}°C (resimtita {Math.Round(averageFeelsLike):0}°C)";
         WindText = $"{Math.Round(Math.Max(0, averageWindSpeed) * 3.6):0} km/h {GetDirectionText(dominantDirection)}";
         HumidityText = $"{Math.Round(Math.Clamp(averageHumidity, 0, 100)):0}%";
         PrecipitationText = $"{Math.Round(Math.Clamp(maxPrecipitation, 0, 100)):0}%";
         CloudinessText = $"{Math.Round(Math.Clamp(averageCloudiness, 0, 100)):0}%";

         NarrativeText = BuildNarrativeText(
            dominantCondition,
            minTemperature,
            maxTemperature,
            averageFeelsLike,
            averageWindSpeed,
            dominantDirection,
            maxPrecipitation,
            averageCloudiness,
            averageHumidity);

         StatusText = string.Empty;
      }
      catch (Exception exception)
      {
         System.Diagnostics.Debug.WriteLine($"Daily details load error: {exception.Message}");
         StatusText = "Nu am putut incarca detaliile meteo.";
         NarrativeText = "A aparut o problema la preluarea datelor din API.";
      }
      finally
      {
         IsBusy = false;
      }
   }

   private string BuildNarrativeText(
      string condition,
      double minTemperature,
      double maxTemperature,
      double averageFeelsLike,
      double averageWindSpeedMetersPerSecond,
      int windDirectionDegrees,
      double precipitation,
      double cloudiness,
      double humidity)
   {
      var period = _isNightMode ? "intervalul de noapte" : "intervalul de zi";
      var windKph = Math.Max(0, averageWindSpeedMetersPerSecond) * 3.6;

      return $"{DayNameText}, {DateText}. Pentru {period}, conditia dominanta este {ToConditionText(condition)}. " +
         $"Temperatura variaza intre {Math.Round(minTemperature):0}°C si {Math.Round(maxTemperature):0}°C, iar temperatura resimtita este in jur de {Math.Round(averageFeelsLike):0}°C. " +
         $"Vantul bate din {GetDirectionText(windDirectionDegrees)} cu aproximativ {Math.Round(windKph):0} km/h, precipitatii {Math.Round(Math.Clamp(precipitation, 0, 100)):0}%, nebulozitate {Math.Round(Math.Clamp(cloudiness, 0, 100)):0}% si umiditate {Math.Round(Math.Clamp(humidity, 0, 100)):0}%.";
   }

   private void ResetMetricValues()
   {
      ConditionText = "-";
      TemperatureText = "-";
      WindText = "-";
      HumidityText = "-";
      PrecipitationText = "-";
      CloudinessText = "-";
   }

   private void UpdateDateLabels()
   {
      var dayName = _culture.DateTimeFormat.GetDayName(_selectedDate.DayOfWeek);
      DayNameText = Capitalize(dayName);
      DateText = _selectedDate.ToString("dd MMMM yyyy", _culture);
   }

   private static string? ReadQueryValue(IDictionary<string, object> query, string key)
   {
      if (query.TryGetValue(key, out var directValue))
      {
         return directValue?.ToString();
      }

      var pair = query.FirstOrDefault(item => string.Equals(item.Key, key, StringComparison.OrdinalIgnoreCase));
      return pair.Equals(default(KeyValuePair<string, object>)) ? null : pair.Value?.ToString();
   }

   private static string ExtractCityDisplay(string cityName)
   {
      return cityName.Split(',').FirstOrDefault()?.Trim() ?? cityName;
   }

   private static string Capitalize(string value)
   {
      if (string.IsNullOrWhiteSpace(value))
      {
         return value;
      }

      if (value.Length == 1)
      {
         return value.ToUpperInvariant();
      }

      return char.ToUpperInvariant(value[0]) + value[1..];
   }

   private async Task GoBackAsync()
   {
      await Shell.Current.GoToAsync("..");
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

   private static string ToConditionText(string condition)
   {
      return condition switch
      {
         "Clear" => "senin",
         "Clouds" => "partial innorat",
         "Rain" => "ploaie",
         "Drizzle" => "burnita",
         "Thunderstorm" => "furtuna",
         "Snow" => "ninsoare",
         "Mist" or "Fog" => "ceata",
         _ => "vreme variabila"
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
