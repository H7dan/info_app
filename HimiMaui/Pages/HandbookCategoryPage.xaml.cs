using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Himi.Pages;

[QueryProperty(nameof(CategoryId), "categoryId")]
public partial class HandbookCategoryPage : ContentPage
{
	private readonly ViewModels.HandbookCategoryViewModel _vm;

	public string? CategoryId { get; set; }

	public HandbookCategoryPage()
	{
		InitializeComponent();
		_vm = App.Services.GetRequiredService<ViewModels.HandbookCategoryViewModel>();
		BindingContext = _vm;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await _vm.LoadAsync(CategoryId);
	}

	private async void OnArticleSelectionChanged(object? sender, SelectionChangedEventArgs e)
	{
		if (e.CurrentSelection.FirstOrDefault() is not ViewModels.ArticleListItemVm item)
			return;

		((CollectionView)sender!).SelectedItem = null;

		await Shell.Current.GoToAsync(
			$"Article?title={Uri.EscapeDataString(item.Title)}&bodyPath={Uri.EscapeDataString(item.BodyPath)}");
	}
}

