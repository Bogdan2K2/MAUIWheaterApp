using MauiAppWeatherModel;
using MauiAppWeatherRepository;
using MauiAppWeatherServices.Interfaces;
using SQLite;
using System.Net.Http.Json;

namespace MauiAppWeatherServices.Implementations
{
   public class HourlyWeatherService(DatabaseContext databaseContext, IHttpClientFactory httpClientFactory) :
      BaseService(databaseContext, httpClientFactory), IHourlyWeatherService
   {
      public async Task<List<HourlyWeather>> GetHourlyWeatherAsync(DateTime dateTime)
      {
            //Se interogheaza baza de date. Daca exista date pentru data curenta(dateTime), atunci se va intoarce
            //inregistrarea care corspunde acestei date.
            //In caz contrar, Se citesc datele folsind un API, apoi se insereaza in baza de date
            //Pentru insert se construieste un obiect de tipul DailyWeather cu datele primite de la API
            //Se va decomenta si modifica codul de mai jos
            /*
            SQLiteAsyncConnection connection = await this.DatabaseContext.Connection;
            await connection.InsertAsync(new DailyWeather() { });
            */
            DateTime dateHourly = dateTime.Date;
            SQLiteAsyncConnection connection = await this.DatabaseContext.Connection;
            HourlyWeather hourlyWeatherDb=await connection.Table<HourlyWeather> ().Where(dataWork => dataWork.Time >= dateHourly).FirstOrDefaultAsync();
            string API = "";
            if (hourlyWeatherDb != null)
            {
                return new List< HourlyWeather > { hourlyWeatherDb };
            }
            else
            {
                try
                {
                    using (HttpClient clientul = new HttpClient())
                    {
                        var dataAPIHourlyWeather = await clientul.GetFromJsonAsync<HourlyWeather>(API);
                        if (dataAPIHourlyWeather != null)
                        {
                            HourlyWeather hW = new HourlyWeather{
                                Time = dataAPIHourlyWeather.Time,
                                Temperature = dataAPIHourlyWeather.Temperature,

                            };
                            await connection.InsertAsync(hW);
                            return new List<HourlyWeather> { hW };
                        }

                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine("Nu avem acele date");
                }
            }

            return new List<HourlyWeather>();
      }
   }
}