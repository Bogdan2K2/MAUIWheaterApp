using MauiAppWeatherClient.ViewModels;
using Microsoft.Maui.Controls.Maps;

namespace MauiAppWeatherClient.Views
{
   public partial class MapPage : ContentPage
   {
      private readonly MapViewModel _viewModel;

      public MapPage(MapViewModel viewModel)
      {
         _viewModel = viewModel;
         InitializeComponent();
      }

      protected override void OnAppearing()
      {
         base.OnAppearing();

         MapControl.Pins.Clear();

         foreach (Pin pin in _viewModel.Pins)
         {
            MapControl.Pins.Add(pin);
         }

         if (_viewModel.MapSpan is not null)
         {
            MapControl.MoveToRegion(_viewModel.MapSpan);
         }
      }
   }
}