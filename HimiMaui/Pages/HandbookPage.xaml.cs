using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Himi.Services;

namespace Himi.Pages;

public partial class HandbookPage : ContentPage
{
	private readonly ViewModels.HandbookViewModel _vm;

	public HandbookPage()
	{
		InitializeComponent();
		_vm = App.Services.GetRequiredService<ViewModels.HandbookViewModel>();
		BindingContext = _vm;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await _vm.LoadAsync();
	}

	private async void OnCategorySelectionChanged(object? sender, SelectionChangedEventArgs e)
	{
		if (e.CurrentSelection.FirstOrDefault() is not ViewModels.HandbookCategoryItemVm item)
			return;

		((CollectionView)sender!).SelectedItem = null;

		var repo = App.Services.GetRequiredService<IContentRepository>();
		var language = App.Services.GetRequiredService<ILanguageService>();
		var index = await repo.GetIndexAsync();

		var articles = index.Articles
			.Where(a => a.Lang == language.CurrentLanguage && a.CategoryId == item.Id)
			.OrderBy(a => a.Title)
			.ToList();

		if (articles.Count == 1)
		{
			var a = articles[0];
			await Shell.Current.GoToAsync(
				$"Article?title={Uri.EscapeDataString(a.Title)}&bodyPath={Uri.EscapeDataString(a.BodyPath)}");
			return;
		}

		await Shell.Current.GoToAsync($"HandbookCategory?categoryId={Uri.EscapeDataString(item.Id)}");
	}
}

