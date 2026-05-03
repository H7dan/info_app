using Microsoft.Extensions.DependencyInjection;

namespace Himi.Pages;

[QueryProperty(nameof(ChecklistId), "checklistId")]
public partial class ChecklistPage : ContentPage
{
	private readonly ViewModels.ChecklistViewModel _vm;

	public string? ChecklistId { get; set; }

	public ChecklistPage()
	{
		InitializeComponent();
		_vm = App.Services.GetRequiredService<ViewModels.ChecklistViewModel>();
		BindingContext = _vm;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await _vm.LoadAsync(ChecklistId);
	}

	private async void OnOpenArticleClicked(object? sender, EventArgs e)
	{
		if (sender is not Button { CommandParameter: ViewModels.ChecklistItemVm item })
			return;

		if (string.IsNullOrWhiteSpace(item.LinksToArticleId))
			return;

		if (!_vm.TryGetArticle(item.LinksToArticleId, out var article))
			return;

		await Shell.Current.GoToAsync(
			$"Article?title={Uri.EscapeDataString(article.Title)}&bodyPath={Uri.EscapeDataString(article.BodyPath)}");
	}
}

