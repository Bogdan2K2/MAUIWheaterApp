using MauiAppWeatherClient.ViewModels;

namespace MauiAppWeatherClient.Views;

public partial class HourlyPage : ContentPage
{
	private readonly HourlyViewModel _viewModel;

	public HourlyPage(HourlyViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = viewModel;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await _viewModel.LoadAsync();
	}
}
