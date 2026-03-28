using Microsoft.Maui.ApplicationModel;

namespace MauiAppWeatherClient.Views;

public partial class AboutPage : ContentPage
{
	public AboutPage()
	{
		InitializeComponent();
	}

	private async void OnEmailTapped(object? sender, TappedEventArgs e)
	{
		if (e.Parameter is not string mailToLink || string.IsNullOrWhiteSpace(mailToLink))
		{
			return;
		}

		try
		{
			await Launcher.Default.OpenAsync(mailToLink);
		}
		catch
		{
			// Ignore if no mail client is available on current platform.
		}
	}
}
