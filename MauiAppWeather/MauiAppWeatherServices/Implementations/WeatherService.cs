using MauiAppWeatherModel;
using MauiAppWeatherServices.Interfaces;
using System.Net.Http.Json;

namespace MauiAppWeatherServices.Implementations
{
    public class WeatherService(HttpClient httpClient) : IWeatherService
    {
        private readonly HttpClient _httpClient = httpClient;
        private const string API_KEY = "5c2d3b19dc0b4d9bf53d6bb96d4bad2a";

        public async Task<List<LocationDto>> SearchAsync(string query)
        {
            string url = $"https://api.openweathermap.org/geo/1.0/direct?q={query}&limit=5&appid={API_KEY}";

            HttpResponseMessage responseMessage = await _httpClient.GetAsync(url);

            if (!responseMessage.IsSuccessStatusCode)
            {
                throw new Exception("API not yet active");
                //return [];
            }

            var result = await responseMessage.Content.ReadFromJsonAsync<List<GeoLocationResponse>>();

            return result?.Select(x => new LocationDto
            {
                CityName = $"{x.Name}, {x.Country}",
                Latitude = x.Lat,
                Longitude = x.Lon,
                ZipCode = x.Zip
            }).ToList() ?? [];
        }
        public async Task<List<HourlyWeather>> GetHourlyWeathersAsync(string cityName)
        {
            try
            {
                var city = cityName.Split(',').FirstOrDefault()?.Trim() ?? cityName;

                string url = $"https://api.openweathermap.org/data/2.5/forecast?q={city}&appid={API_KEY}&units=metric";

                HttpResponseMessage responseMessage = await _httpClient.GetAsync(url);

                if (!responseMessage.IsSuccessStatusCode)
                {
                    return new List<HourlyWeather>();
                }

                var jsonResponse = await responseMessage.Content.ReadFromJsonAsync<WeatherResponseForecast>();

                if (jsonResponse != null && jsonResponse.List != null)
                {
                    var resulted = jsonResponse.List.Select(x => new HourlyWeather
                    {
                        Time = DateTime.Parse(x.DataText),
                        Temperature = x.Main?.Temperature ?? 0,
                        FeelsLikeTemperature = x.Main?.FeelsLike ?? 0,
                        PrecipitationProbability = x.PrecipitationProbability * 100,
                        WindSpeed = x.Wind?.Speed ?? 0,
                        WindDirectionDegrees = x.Wind?.Degree ?? 0,
                        Humidity = x.Main?.Humidity ?? 0,
                        Cloudiness = x.Clouds?.All ?? 0,
                        Condition = x.Weather?.FirstOrDefault()?.Main ?? "Clear",
                        IconCode = x.Weather?.FirstOrDefault()?.Icon ?? "01d"
                    }).ToList();

                    return resulted;
                }

                return new List<HourlyWeather>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error meteo: {ex.Message}");

                return new List<HourlyWeather>();
            }
        }
        public async Task<DailyWeather> GetDailyWeatherAsync(string cityName)
        {
            var city = cityName.Split(',').FirstOrDefault()?.Trim() ?? cityName;
            string url = $"https://api.openweathermap.org/data/2.5/forecast?q={city}&appid={API_KEY}&units=metric";

            var response = await _httpClient.GetFromJsonAsync<WeatherResponseForecast>(url);

            if (response?.List != null && response.List.Any())
            {
                var dateRelevante = response.List.Take(8).ToList();

                return new DailyWeather
                {
                    Date = DateTime.Today,
                    MinTemperature = dateRelevante.Min(x => x.Main.Temperature),
                    MaxTemperature = dateRelevante.Max(x => x.Main.Temperature),
                    Condition = dateRelevante.FirstOrDefault()?.Weather?.FirstOrDefault()?.Main ?? "Clear"
                };
            }

            return null;
        }
    }
}
