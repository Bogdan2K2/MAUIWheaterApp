# MAUI Weather App

This repository contains a multi-project .NET MAUI weather application.

## Project Structure

- `MauiAppWeather/MauiAppWeatherClient` - MAUI client app (UI, ViewModels, navigation)
- `MauiAppWeather/MauiAppWeatherServices` - service layer for weather/settings/location logic
- `MauiAppWeather/MauiAppWeatherRepository` - repository/data access layer
- `MauiAppWeather/MauiAppWeatherModel` - shared DTO/model definitions
- `MauiAppWeather/MauiAppWeather.slnx` - solution file

## Requirements

- .NET 10 SDK
- MAUI workloads installed
- Windows (for `net10.0-windows10.0.19041.0`) or Android tooling (for `net10.0-android`)

## Run (Windows)

```powershell
dotnet build .\MauiAppWeather\MauiAppWeather.slnx -c Debug
dotnet run --project .\MauiAppWeather\MauiAppWeatherClient\MauiAppWeatherClient.csproj -f net10.0-windows10.0.19041.0
```

## Run (Android)

```powershell
dotnet build .\MauiAppWeather\MauiAppWeather.slnx -c Debug
dotnet run --project .\MauiAppWeather\MauiAppWeatherClient\MauiAppWeatherClient.csproj -f net10.0-android
```

## Screenshots

### Splash Screen

![Splash Screen](MauiAppWeather/MauiAppWeatherClient/Resources/Splash/splash.svg)

### App Icon

![App Icon](MauiAppWeather/MauiAppWeatherClient/Resources/AppIcon/appicon.svg)

### In-App Asset

![In-App Asset](MauiAppWeather/MauiAppWeatherClient/Resources/Images/dotnet_bot.png)

## License

This project is distributed under the terms described in [EULA.md](./EULA.md).
