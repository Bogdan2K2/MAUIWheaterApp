using MauiAppWeatherClient.ViewModels;
using System.Globalization;
using System.Text.Json;

namespace MauiAppWeatherClient.Views
{
   public partial class MapPage : ContentPage
   {
      private const string TomTomApiKey = "e49t8Wvh1pKOlzoAQLdWEjNfKf8TxdPE";

      private readonly MapViewModel _viewModel;

      public MapPage(MapViewModel viewModel)
      {
         _viewModel = viewModel;
         InitializeComponent();
      }

      protected override void OnAppearing()
      {
         base.OnAppearing();
         _ = RefreshMapAsync();
      }

      private async Task RefreshMapAsync()
      {
         try
         {
            await _viewModel.LoadAsync();

            if (_viewModel.MapSpan is null || _viewModel.Pins.Count == 0)
            {
               MapWebView.IsVisible = false;
               EmptyStateContainer.IsVisible = true;
               return;
            }

            List<object> markers = _viewModel.Pins.Select(pin => new
            {
               label = pin.Label,
               latitude = pin.Location.Latitude,
               longitude = pin.Location.Longitude,
            }).Cast<object>().ToList();

            string markersJson = JsonSerializer.Serialize(markers);
            string centerLatitude = _viewModel.MapSpan.Center.Latitude.ToString(CultureInfo.InvariantCulture);
            string centerLongitude = _viewModel.MapSpan.Center.Longitude.ToString(CultureInfo.InvariantCulture);
            string staticMapUrl = BuildTomTomStaticMapUrl(centerLatitude, centerLongitude);

            MapWebView.Source = new HtmlWebViewSource
            {
               Html = BuildTomTomMapHtml(centerLatitude, centerLongitude, markersJson, staticMapUrl)
            };

            EmptyStateContainer.IsVisible = false;
            MapWebView.IsVisible = true;
         }
         catch
         {
            MapWebView.IsVisible = false;
            EmptyStateContainer.IsVisible = true;
         }
      }

      private static string BuildTomTomMapHtml(string centerLatitude, string centerLongitude, string markersJson, string staticMapUrl)
      {
         const string template = """
<!DOCTYPE html>
<html>
<head>
  <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no">
  <link rel="stylesheet" type="text/css" href="https://unpkg.com/@tomtom-international/web-sdk-maps/dist/maps.css">
  <style>
    html, body, #map {
      margin: 0;
      padding: 0;
      width: 100%;
      height: 100%;
    }
    #map {
      position: relative;
      overflow: hidden;
    }
    #fallback-layer {
      position: absolute;
      inset: 0;
      background-image: url('__STATIC_MAP_URL__');
      background-size: cover;
      background-position: center;
      background-repeat: no-repeat;
    }
    #fallback-pin {
      position: absolute;
      left: 50%;
      top: 50%;
      width: 18px;
      height: 18px;
      border-radius: 50%;
      background-color: #e53935;
      border: 2px solid #ffffff;
      box-shadow: 0 0 0 2px rgba(0, 0, 0, 0.2);
      transform: translate(-50%, -50%);
    }
  </style>
</head>
<body>
  <div id="map">
    <div id="fallback-layer"></div>
    <div id="fallback-pin"></div>
  </div>
  <script src="https://unpkg.com/@tomtom-international/web-sdk-maps/dist/maps-web.min.js"></script>
  <script>
    const fallbackLayer = document.getElementById('fallback-layer');
    const fallbackPin = document.getElementById('fallback-pin');
    const showFallback = () => {
      if (fallbackLayer) {
        fallbackLayer.style.display = 'block';
      }
      if (fallbackPin) {
        fallbackPin.style.display = 'block';
      }
    };
    const hideFallback = () => {
      if (fallbackLayer) {
        fallbackLayer.style.display = 'none';
      }
      if (fallbackPin) {
        fallbackPin.style.display = 'none';
      }
    };

    window.onerror = () => {
      showFallback();
      return false;
    };

    if (typeof tt === 'undefined' || typeof tt.map !== 'function') {
      showFallback();
    } else {
      try {
        const map = tt.map({
          key: '__TOMTOM_KEY__',
          container: 'map',
          center: [__CENTER_LONGITUDE__, __CENTER_LATITUDE__],
          zoom: 10
        });

        map.on('load', () => hideFallback());
        map.on('error', () => showFallback());

        map.addControl(new tt.NavigationControl());

        const markers = __MARKERS_JSON__;
        markers.forEach((markerData) => {
          const marker = new tt.Marker()
            .setLngLat([markerData.longitude, markerData.latitude])
            .addTo(map);

          if (markerData.label) {
            marker.setPopup(new tt.Popup({ offset: 25 }).setText(markerData.label));
          }
        });
      } catch (error) {
        showFallback();
      }
    }
  </script>
</body>
</html>
""";

         return template
            .Replace("__TOMTOM_KEY__", TomTomApiKey)
            .Replace("__CENTER_LONGITUDE__", centerLongitude)
            .Replace("__CENTER_LATITUDE__", centerLatitude)
            .Replace("__MARKERS_JSON__", markersJson)
            .Replace("__STATIC_MAP_URL__", staticMapUrl);
      }

      private static string BuildTomTomStaticMapUrl(string centerLatitude, string centerLongitude)
      {
         return $"https://api.tomtom.com/map/1/staticimage?layer=basic&style=main&format=png&zoom=10&center={centerLongitude},{centerLatitude}&width=1080&height=1920&view=Unified&key={TomTomApiKey}";
      }
   }
}
