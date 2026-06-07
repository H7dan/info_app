using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Himi.Models;
using Himi.Services;

namespace Himi.ViewModels;

public sealed class ChatMessageVm
{
	public required string Role { get; init; }
	public required string Content { get; init; }
	public bool IsUser => Role == "user";
	public string DisplayText => IsUser ? $"▶ {Content}" : Content;
}

public sealed class ChatViewModel : INotifyPropertyChanged
{
	private readonly IAssistantService _assistant;
	private readonly IChatHistoryStore _historyStore;
	private readonly IContentRepository _content;
	private readonly INewsStore _newsStore;
	private readonly ILanguageService _language;
	private readonly IUiStringsService _strings;
	private readonly IAiSettingsService _aiSettings;

	private string _input = string.Empty;
	private bool _isBusy;
	private string? _articleId;
	private string? _articleTitle;
	private string? _articleBodyPath;
	private string _articleMarkdown = string.Empty;
	private CancellationTokenSource? _cts;

	public event PropertyChangedEventHandler? PropertyChanged;

	public ObservableCollection<ChatMessageVm> Messages { get; } = new();

	public string Input
	{
		get => _input;
		set
		{
			if (_input == value) return;
			_input = value;
			OnPropertyChanged();
			OnPropertyChanged(nameof(CanSend));
			((Command)SendCommand).ChangeCanExecute();
		}
	}

	public bool IsBusy
	{
		get => _isBusy;
		private set
		{
			if (_isBusy == value) return;
			_isBusy = value;
			OnPropertyChanged();
			OnPropertyChanged(nameof(CanSend));
			OnPropertyChanged(nameof(CanCancel));
		}
	}

	public bool HasArticleContext => !string.IsNullOrWhiteSpace(_articleTitle);

	public string ArticleFootnote => string.IsNullOrWhiteSpace(_articleTitle) ? string.Empty : $"— {_articleTitle}";

	public string Disclaimer => _strings.ChatDisclaimer;

	public string ChatTitle => _strings.ChatTitle;

	public string InputPlaceholder => _strings.ChatInputPlaceholder;

	public string SendLabel => _strings.ChatSend;

	public string CancelLabel => _strings.Cancel;

	public bool CanSend => !IsBusy && !string.IsNullOrWhiteSpace(Input);
	public bool CanCancel => IsBusy;

	public ICommand SendCommand { get; }
	public ICommand CancelCommand { get; }

	public ChatViewModel(
		IAssistantService assistant,
		IChatHistoryStore historyStore,
		IContentRepository content,
		INewsStore newsStore,
		ILanguageService language,
		IUiStringsService strings,
		IAiSettingsService aiSettings)
	{
		_assistant = assistant;
		_historyStore = historyStore;
		_content = content;
		_newsStore = newsStore;
		_language = language;
		_strings = strings;
		_aiSettings = aiSettings;

		SendCommand = new Command(async () => await SendAsync(), () => CanSend);
		CancelCommand = new Command(Cancel, () => CanCancel);

		_strings.PropertyChanged += (_, _) => NotifyLocalizedStringsChanged();
	}

	private void NotifyLocalizedStringsChanged()
	{
		OnPropertyChanged(nameof(ChatTitle));
		OnPropertyChanged(nameof(InputPlaceholder));
		OnPropertyChanged(nameof(SendLabel));
		OnPropertyChanged(nameof(CancelLabel));
		OnPropertyChanged(nameof(Disclaimer));
	}

	public async Task LoadAsync(string? articleId, string? bodyPath, string? title)
	{
		_articleId = articleId;
		_articleTitle = title;
		_articleBodyPath = bodyPath;
		_articleMarkdown = string.Empty;

		// Prefer articleId lookup — more reliable than bodyPath in Shell query string.
		if (!string.IsNullOrWhiteSpace(articleId))
		{
			var index = await _newsStore.LoadIndexAsync();
			var item = index.Items.FirstOrDefault(i =>
				string.Equals(i.Id, articleId, StringComparison.OrdinalIgnoreCase));
			if (item is not null)
			{
				_articleTitle = item.Title;
				_articleBodyPath = item.BodyPath;
				_articleMarkdown = await _content.GetArticleMarkdownAsync(item.BodyPath);
			}
		}

		if (string.IsNullOrWhiteSpace(_articleMarkdown) && !string.IsNullOrWhiteSpace(bodyPath))
			_articleMarkdown = await _content.GetArticleMarkdownAsync(bodyPath);

		if (!string.IsNullOrWhiteSpace(title))
			_articleTitle = title;

		Messages.Clear();
		var saved = await _historyStore.LoadAsync();
		foreach (var m in saved)
			Messages.Add(new ChatMessageVm { Role = m.Role, Content = m.Content });

		OnPropertyChanged(nameof(HasArticleContext));
		OnPropertyChanged(nameof(ArticleFootnote));
	}

	private async Task SendAsync()
	{
		if (IsBusy || string.IsNullOrWhiteSpace(Input))
			return;

		if (!_aiSettings.StubMode && Connectivity.NetworkAccess != NetworkAccess.Internet)
		{
			await Shell.Current.DisplayAlertAsync(_strings.Error, _strings.NoInternet, _strings.Ok);
			return;
		}

		if (HasArticleContext && string.IsNullOrWhiteSpace(_articleMarkdown))
		{
			await Shell.Current.DisplayAlertAsync(_strings.Error, _strings.ArticleContextMissing, _strings.Ok);
			return;
		}

		var userText = Input.Trim();
		Input = string.Empty;
		((Command)SendCommand).ChangeCanExecute();

		Messages.Add(new ChatMessageVm { Role = "user", Content = userText });
		await PersistHistoryAsync();

		IsBusy = true;
		_cts = new CancellationTokenSource();
		((Command)CancelCommand).ChangeCanExecute();

		try
		{
			var history = BuildHistory();
			string reply;

			// `CurrentLanguage` is only a fallback hint for the server; reply language follows the user's message.
			if (HasArticleContext && !string.IsNullOrWhiteSpace(_articleMarkdown))
			{
				reply = await _assistant.ChatAboutArticleAsync(
					userText,
					_language.CurrentLanguage,
					_articleTitle ?? string.Empty,
					_articleMarkdown,
					history,
					_cts.Token);
			}
			else
			{
				reply = await _assistant.ChatAsync(
					userText,
					_language.CurrentLanguage,
					history,
					_cts.Token);
			}

			Messages.Add(new ChatMessageVm { Role = "assistant", Content = reply });
			await PersistHistoryAsync();
		}
		catch (OperationCanceledException)
		{
			// User cancelled — keep partial history as-is.
		}
		catch (Exception ex)
		{
			var message = FormatErrorMessage(ex);
			await Shell.Current.DisplayAlertAsync(_strings.Error, message, _strings.Ok);
		}
		finally
		{
			_cts?.Dispose();
			_cts = null;
			IsBusy = false;
			((Command)SendCommand).ChangeCanExecute();
			((Command)CancelCommand).ChangeCanExecute();
		}
	}

	private void Cancel()
	{
		_cts?.Cancel();
	}

	private IReadOnlyList<ChatTurn> BuildHistory()
	{
		// Exclude the latest user message — it is sent as `message`.
		var all = Messages.ToList();
		if (all.Count == 0)
			return Array.Empty<ChatTurn>();

		var slice = all.Count > 1 ? all.Take(all.Count - 1) : Enumerable.Empty<ChatMessageVm>();
		return slice
			.Select(m => new ChatTurn(m.Role, m.Content))
			.ToList();
	}

	private async Task PersistHistoryAsync()
	{
		var messages = Messages
			.Select(m => new ChatMessage(m.Role, m.Content, DateTime.UtcNow))
			.ToList();
		await _historyStore.SaveAsync(messages);
	}

	private string FormatErrorMessage(Exception ex)
	{
		if (IsConnectionError(ex))
			return _strings.ConnectionFailed;

		if (ex.Message.Contains("502", StringComparison.OrdinalIgnoreCase)
			|| ex.Message.Contains("500", StringComparison.OrdinalIgnoreCase)
			|| ex.Message.Contains("status code", StringComparison.OrdinalIgnoreCase))
			return $"{_strings.ServerError}\n{ex.Message}";

		return ex.Message;
	}

	private static bool IsConnectionError(Exception ex)
	{
		var msg = ex.Message;
		if (msg.Contains("502", StringComparison.OrdinalIgnoreCase)
			|| msg.Contains("500", StringComparison.OrdinalIgnoreCase)
			|| msg.Contains("status code", StringComparison.OrdinalIgnoreCase))
			return false;

		for (var cur = ex; cur is not null; cur = cur.InnerException)
		{
			if (cur is HttpRequestException { StatusCode: not null })
				return false;
		}

		return msg.Contains("connection failure", StringComparison.OrdinalIgnoreCase)
			|| msg.Contains("failed to connect", StringComparison.OrdinalIgnoreCase)
			|| msg.Contains("network is unreachable", StringComparison.OrdinalIgnoreCase);
	}

	private void OnPropertyChanged([CallerMemberName] string? name = null)
		=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
