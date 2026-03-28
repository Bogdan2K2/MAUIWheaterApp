namespace MauiAppWeatherClient
{
   public partial class AppShell : Shell
   {
      public AppShell()
      {
         InitializeComponent();

         Routing.RegisterRoute(nameof(Views.SettingsPage), typeof(Views.SettingsPage));
         Routing.RegisterRoute(nameof(Views.NewLocationPage), typeof(Views.NewLocationPage));
         Routing.RegisterRoute(nameof(Views.DailyDetailsContentPage), typeof(Views.DailyDetailsContentPage));
      }

      async void OnSettingsClicked(object sender, EventArgs e)
      {
         await Shell.Current.GoToAsync(nameof(Views.SettingsPage));
      }
   }
}
