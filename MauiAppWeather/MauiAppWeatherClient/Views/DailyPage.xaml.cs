using MauiAppWeatherClient.ViewModels;

namespace MauiAppWeatherClient.Views;

public partial class DailyPage : ContentPage
{
   private readonly DailyViewModel _viewModel;

   public DailyPage(DailyViewModel viewModel)
   {
      InitializeComponent();
      _viewModel = viewModel;
      BindingContext = viewModel;
   }

   protected override async void OnAppearing()
   {
      base.OnAppearing();
      await _viewModel.LoadAsync();
   }
}
