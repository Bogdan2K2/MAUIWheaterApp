using MauiAppWeatherRepository;

namespace MauiAppWeatherServices
{
   public abstract class BaseService(DatabaseContext databaseContext, IHttpClientFactory httpClientFactory)
   {
      protected readonly HttpClient _httpClient = httpClientFactory.CreateClient();

      protected DatabaseContext DatabaseContext { get; } = databaseContext;
   }
}
