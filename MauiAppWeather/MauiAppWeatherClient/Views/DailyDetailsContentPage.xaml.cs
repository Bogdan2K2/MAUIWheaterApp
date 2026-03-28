using MauiAppWeatherClient.ViewModels;

namespace MauiAppWeatherClient.Views;

public partial class DailyDetailsContentPage : ContentPage
{
   public DailyDetailsContentPage(DailyDetailsViewModel viewModel)
   {
      InitializeComponent();
      BindingContext = viewModel;
   }
}
