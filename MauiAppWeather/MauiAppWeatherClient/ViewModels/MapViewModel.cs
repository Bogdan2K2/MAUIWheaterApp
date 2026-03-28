using CommunityToolkit.Mvvm.ComponentModel;
using MauiAppWeatherServices.Interfaces;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace MauiAppWeatherClient.ViewModels
{
   public partial class MapViewModel : ObservableObject
   {
      private ILocationsService _locationsService;

      public MapViewModel(ILocationsService locationsService)
      {
         this._locationsService = locationsService;

         Pins = new List<Pin>();
         LoadAsync();
      }

      [ObservableProperty]
      private List<Pin> pins;

      [ObservableProperty]
      private MapSpan mapSpan;

      private async void LoadAsync()
      {
         List<MauiAppWeatherModel.Location> locations = await _locationsService.GetLocationsAsync();

         if (locations?.Count == 0)
         {
            return;
         }

         MauiAppWeatherModel.Location current = locations.First();
         Location currentLocation = new Location(current.Latitude, current.Longitude);
         MapSpan = MapSpan.FromCenterAndRadius(currentLocation, Distance.FromKilometers(20));

         List<Pin> pinList = [];

         foreach (MauiAppWeatherModel.Location location in locations)
         {
            double distance = Location.CalculateDistance(current.Latitude, current.Longitude,
               location.Latitude, location.Longitude, DistanceUnits.Kilometers);

            if(distance <= 50)
            {
               pinList.Add(new Pin()
               {
                  Label = location.CityName,
                  Location = new Location(location.Latitude, location.Longitude),
               });
            }
         }

         Pins = pinList;
      }
   }
}
