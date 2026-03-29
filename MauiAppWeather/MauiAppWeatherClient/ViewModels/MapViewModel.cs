using CommunityToolkit.Mvvm.ComponentModel;
using MauiAppWeatherServices.Interfaces;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace MauiAppWeatherClient.ViewModels
{
   public partial class MapViewModel : ObservableObject
   {
      private readonly ILocationsService _locationsService;

      public MapViewModel(ILocationsService locationsService)
      {
         _locationsService = locationsService;
         Pins = new List<Pin>();
      }

      [ObservableProperty]
      private List<Pin> pins;

      [ObservableProperty]
      private MapSpan? mapSpan;

      public async Task LoadAsync()
      {
         List<MauiAppWeatherModel.Location> locations = await _locationsService.GetLocationsAsync() ?? [];
         locations = locations.Where(location => location.Active).ToList();

         if (locations.Count == 0)
         {
            Pins = new List<Pin>();
            MapSpan = null;
            return;
         }

         MauiAppWeatherModel.Location current = locations.FirstOrDefault(location => location.IsCurrent)
             ?? locations.First();

         Location currentLocation = new Location(current.Latitude, current.Longitude);
         MapSpan = MapSpan.FromCenterAndRadius(currentLocation, Distance.FromKilometers(20));

         List<Pin> pinList = [];

         foreach (MauiAppWeatherModel.Location location in locations)
         {
            double distance = Location.CalculateDistance(current.Latitude, current.Longitude,
               location.Latitude, location.Longitude, DistanceUnits.Kilometers);

            if (distance <= 50)
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
