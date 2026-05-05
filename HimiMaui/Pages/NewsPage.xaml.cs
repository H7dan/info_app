using Himi.ViewModels;

namespace Himi.Pages;

public partial class NewsPage : ContentPage
{
	private readonly NewsViewModel _vm;

	public NewsPage()
	{
		InitializeComponent();
		_vm = App.Services.GetRequiredService<NewsViewModel>();
		BindingContext = _vm;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await _vm.LoadCachedAsync();
	}

	private async void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
	{
		if (e.CurrentSelection.FirstOrDefault() is not NewsListItemVm item)
			return;

		// Clear selection immediately for better UX.
		if (sender is CollectionView cv)
			cv.SelectedItem = null;

		await Shell.Current.GoToAsync(
			$"Article?title={Uri.EscapeDataString(item.Title)}&bodyPath={Uri.EscapeDataString(item.BodyPath)}");
	}
}

