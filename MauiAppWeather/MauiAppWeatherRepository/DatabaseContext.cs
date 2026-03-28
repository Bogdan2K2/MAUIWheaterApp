using MauiAppWeatherModel;
using SQLite;

namespace MauiAppWeatherRepository
{
   public class DatabaseContext
   {
      private readonly string _connectionString;

      private readonly Lazy<Task<SQLiteAsyncConnection>> _connection;

      public Task<SQLiteAsyncConnection> Connection => _connection.Value;

      public DatabaseContext()
      {
         this._connectionString = Path.Combine(FileSystem.AppDataDirectory, "cursvalutar.db3");
         this._connection = new Lazy<Task<SQLiteAsyncConnection>>(InitializeAsync);
      }

      private async Task<SQLiteAsyncConnection> InitializeAsync()
      {
         SQLiteAsyncConnection connection = new SQLiteAsyncConnection(this._connectionString);
         Task[] tasks = [connection.CreateTableAsync<MauiAppWeatherModel.Location>(),
            connection.CreateTableAsync<DailyWeather>(),
            connection.CreateTableAsync<HourlyWeather>(),
         connection.CreateTableAsync<UserSettings>()];

         await Task.WhenAll(tasks).ConfigureAwait(false);

         return connection;
      }
   }
}
