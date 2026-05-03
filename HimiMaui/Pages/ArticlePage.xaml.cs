using Microsoft.Extensions.DependencyInjection;

namespace Himi.Pages;

[QueryProperty(nameof(TitleParam), "title")]
[QueryProperty(nameof(BodyPath), "bodyPath")]
public partial class ArticlePage : ContentPage
{
	private readonly ViewModels.ArticleViewModel _vm;

	public string? TitleParam { get; set; }
	public string? BodyPath { get; set; }

	public ArticlePage()
	{
		InitializeComponent();
		_vm = App.Services.GetRequiredService<ViewModels.ArticleViewModel>();
		BindingContext = _vm;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await _vm.LoadAsync(TitleParam, BodyPath);
	}
}

