using MauiAppWeatherClient.ViewModels;

namespace MauiAppWeatherClient.Views
{
   public partial class NewLocationPage : ContentPage
   {
      public NewLocationPage(NewLocationViewModel viewModel)
      {
         InitializeComponent();
         BindingContext = viewModel;
      }
   }
}