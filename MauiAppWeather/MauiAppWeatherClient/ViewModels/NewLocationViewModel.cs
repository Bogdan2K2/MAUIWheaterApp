using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiAppWeatherModel;
using MauiAppWeatherServices.Interfaces;
using Location = MauiAppWeatherModel.Location;

namespace MauiAppWeatherClient.ViewModels;

public partial class NewLocationViewModel : ObservableObject
{
   private readonly ILocationsService _locationsService;
   private readonly IWeatherService _geoService;

   public NewLocationViewModel(ILocationsService locationsService, IWeatherService geoService)
   {
      _locationsService = locationsService;
      _geoService = geoService;

      LoadSavedLocationsAsync();
   }

   [ObservableProperty]
   private string searchText;

   [ObservableProperty]
   private List<LocationDto> searchResult = [];

   [ObservableProperty]
   private List<Location> savedLocations = [];

   private async void LoadSavedLocationsAsync()
   {
      var list = await _locationsService.GetLocationsAsync();
      SavedLocations = list?.Where(l => l.Active).ToList() ?? [];
   }

   [RelayCommand]
   private async Task SearchAsync()
   {
      if (string.IsNullOrWhiteSpace(SearchText))
      {
         return;
      }

      SearchResult = await _geoService.SearchAsync(SearchText);
   }

   [RelayCommand]
   private async Task AddAsync(LocationDto location)
   {
      if (location is null)
      {
         return;
      }

      var existing = await _locationsService.GetByCoordinatesAsync(location.Latitude, location.Longitude);

      if (existing is not null)
      {
         // reactivare
         existing.Active = true;
         await _locationsService.UpdateAsync(existing);
      }
      else
      {
         LocationDto newLocation = new()
         {
            CityName = location.CityName,
            ZipCode = location.ZipCode,
            Latitude = location.Latitude,
            Longitude = location.Longitude
         };

         await _locationsService.AddLocationAsync(newLocation);
      }

      LoadSavedLocationsAsync();
   }

   [RelayCommand]
   private async Task SelectAsync(Location location)
   {
      if (location == null)
      {
         return;
      }

      await _locationsService.SetCurrentLocationAsync(location.Id);

      await Shell.Current.GoToAsync(".."); // back
   }

   [RelayCommand]
   private async Task DeleteAsync(Location location)
   {
      if (location is null)
      {
         return;
      }

      location.Active = false;
      
      await _locationsService.UpdateAsync(location);

      LoadSavedLocationsAsync();
   }
}