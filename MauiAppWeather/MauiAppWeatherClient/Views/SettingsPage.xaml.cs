using MauiAppWeatherClient.ViewModels;

namespace MauiAppWeatherClient.Views
{
   public partial class SettingsPage : ContentPage
   {
      public SettingsPage(SettingsViewModel viewModel)
      {
         InitializeComponent();
         BindingContext = viewModel;
      }
   }
}