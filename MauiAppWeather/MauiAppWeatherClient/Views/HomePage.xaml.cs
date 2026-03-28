using MauiAppWeatherClient.ViewModels;

namespace MauiAppWeatherClient.Views
{
   public partial class HomePage : ContentPage
   {
      private readonly HomePageViewModel _viewModel;

      public HomePage(HomePageViewModel viewModel)
      {
         InitializeComponent();
         _viewModel = viewModel;
         BindingContext = viewModel;

      }
      protected override async void OnAppearing()
      {
         base.OnAppearing();
         await _viewModel.LoadLocationsAsync();
      }
   }
}