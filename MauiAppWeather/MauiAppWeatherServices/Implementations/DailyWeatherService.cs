
using MauiAppWeatherModel;
using MauiAppWeatherRepository;
using MauiAppWeatherServices.Interfaces;
using SQLite;
using System.Net.Http.Json;

namespace MauiAppWeatherServices.Implementations
{
   public class DailyWeatherService(DatabaseContext databaseContext, IHttpClientFactory httpClientFactory) :
      BaseService(databaseContext, httpClientFactory),
      IDailyWeatherService
   {
      public async Task<List<DailyWeather>> GetDailyWeatherAsync(DateTime dateTime)
      {
            //Se interogheaza baza de date. Daca exista date pentru data curenta(dateTime), atunci se va intoarce
            //inregistrarea care corspunde acestei date.
            //In caz contrar, Se citesc datele folsind un API, apoi se insereaza in baza de date
            //Pentru insert se construieste un obiect de tipul DailyWeather cu datele primite de la API
            //Se va decomenta si modifica codul de mai jos
            /*
            SQLiteAsyncConnection connection = await this.DatabaseContext.Connection;
            await connection.InsertAsync(new HourlyWeather() { });
            */
            DateTime dateHourly = dateTime.Date;
            SQLiteAsyncConnection connection = await this.DatabaseContext.Connection;
            DailyWeather hourlyWeatherDb = await connection.Table<DailyWeather>().Where(dataWork => dataWork.Date >= dateHourly).FirstOrDefaultAsync();
            string API = "";
            if (hourlyWeatherDb != null)
            {
                return new List<DailyWeather> { hourlyWeatherDb };
            }
            else
            {
                try
                {
                    using (HttpClient clientul = new HttpClient())
                    {
                        var dataAPIHourlyWeather = await clientul.GetFromJsonAsync<DailyWeather>(API);
                        if (dataAPIHourlyWeather != null)
                        {
                            DailyWeather hW = new DailyWeather
                            {
                                Date = dataAPIHourlyWeather.Date,
                                MaxTemperature = dataAPIHourlyWeather.MaxTemperature,
                                MinTemperature = dataAPIHourlyWeather.MinTemperature,
                                Condition = dataAPIHourlyWeather.Condition
                                

                            };
                            await connection.InsertAsync(hW);

                            return new List<DailyWeather> { hW };
                        }

                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Nu avem acele date");
                }
            }
            return new List<DailyWeather>();
      }
   }
}
