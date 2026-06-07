using Microsoft.Extensions.DependencyInjection;

namespace Himi.Pages;

[QueryProperty(nameof(ArticleIdParam), "articleId")]
[QueryProperty(nameof(BodyPathParam), "bodyPath")]
[QueryProperty(nameof(TitleParam), "title")]
public partial class ChatPage : ContentPage
{
	private readonly ViewModels.ChatViewModel _vm;

	public string? ArticleIdParam { get; set; }
	public string? BodyPathParam { get; set; }
	public string? TitleParam { get; set; }

	public ChatPage()
	{
		InitializeComponent();
		_vm = App.Services.GetRequiredService<ViewModels.ChatViewModel>();
		BindingContext = _vm;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await _vm.LoadAsync(ArticleIdParam, BodyPathParam, TitleParam);
	}

	private void OnChatInputCompleted(object? sender, EventArgs e)
	{
		if (_vm.CanSend && _vm.SendCommand.CanExecute(null))
			_vm.SendCommand.Execute(null);
	}
}
