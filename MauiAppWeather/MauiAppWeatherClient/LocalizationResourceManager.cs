using MauiAppWeatherClient.Resources.Localization;
using System.Globalization;

namespace MauiAppWeatherClient
{
   public static class LocalizationResourceManager
   {
      public static List<String> Languages { get; set; }

      public static void SetCulture(string languageCode)
      {
         CultureInfo cultureInfo = CultureInfo.GetCultureInfo(languageCode);

         Thread.CurrentThread.CurrentCulture = cultureInfo;
         Thread.CurrentThread.CurrentUICulture = cultureInfo;

         AppResources.Culture = cultureInfo;

         Languages = languageCode == "en" ? ["English", "Romanian"] : ["Engleză", "Română"];
      }
   }
}