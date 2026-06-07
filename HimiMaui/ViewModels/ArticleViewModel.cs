using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Himi.Models;
using Himi.Services;

namespace Himi.ViewModels;

public sealed class ArticleViewModel : INotifyPropertyChanged
{
	private readonly IContentRepository _contentRepository;
	private readonly IMarkdownRenderer _markdown;
	private readonly INewsStore _newsStore;
	private readonly ITranslationService _translation;
	private readonly ILanguageService _language;
	private readonly IUiStringsService _strings;
	private readonly IAiSettingsService _aiSettings;

	private string _title = string.Empty;
	private string _originalTitle = string.Empty;
	private string _bodyPath = string.Empty;
	private string? _articleId;
	private string _originalMarkdown = string.Empty;
	private HtmlWebViewSource _htmlSource = new();
	private bool _isLoading;
	private bool _isBusy;
	private bool _isNewsArticle;
	private bool _hasTranslation;
	private bool _showTranslation;

	public event PropertyChangedEventHandler? PropertyChanged;

	public string Title
	{
		get => _title;
		private set { if (_title == value) return; _title = value; OnPropertyChanged(); }
	}

	public HtmlWebViewSource HtmlSource
	{
		get => _htmlSource;
		private set { if (ReferenceEquals(_htmlSource, value)) return; _htmlSource = value; OnPropertyChanged(); }
	}

	public bool IsLoading
	{
		get => _isLoading;
		private set { if (_isLoading == value) return; _isLoading = value; OnPropertyChanged(); }
	}

	public bool IsBusy
	{
		get => _isBusy;
		private set
		{
			if (_isBusy == value) return;
			_isBusy = value;
			OnPropertyChanged();
			OnPropertyChanged(nameof(CanTranslate));
			OnPropertyChanged(nameof(CanAsk));
			((Command)TranslateCommand).ChangeCanExecute();
			((Command)AskQuestionCommand).ChangeCanExecute();
		}
	}

	public bool IsNewsArticle
	{
		get => _isNewsArticle;
		private set { if (_isNewsArticle == value) return; _isNewsArticle = value; OnPropertyChanged(); OnPropertyChanged(nameof(ShowNewsActions)); }
	}

	public bool ShowNewsActions => IsNewsArticle;

	public bool HasTranslation
	{
		get => _hasTranslation;
		private set
		{
			if (_hasTranslation == value) return;
			_hasTranslation = value;
			OnPropertyChanged();
			OnPropertyChanged(nameof(ShowTranslationToggle));
			((Command)ToggleTranslationCommand).ChangeCanExecute();
		}
	}

	public bool ShowTranslationToggle => IsNewsArticle && HasTranslation;

	public bool ShowTranslation
	{
		get => _showTranslation;
		set
		{
			if (_showTranslation == value) return;
			_showTranslation = value;
			OnPropertyChanged();
			OnPropertyChanged(nameof(TranslationToggleLabel));
			_ = ApplyViewModeAsync();
		}
	}

	public string TranslationToggleLabel => ShowTranslation ? _strings.ViewOriginal : _strings.ViewTranslation;

	public string TranslateLabel => _strings.TranslateArticle;

	public string AskQuestionLabel => _strings.AskAboutArticle;

	public bool CanTranslate => IsNewsArticle && !IsBusy;
	public bool CanAsk => IsNewsArticle && !IsBusy && !string.IsNullOrWhiteSpace(_articleId);

	public ICommand TranslateCommand { get; }
	public ICommand AskQuestionCommand { get; }
	public ICommand ToggleTranslationCommand { get; }

	public ArticleViewModel(
		IContentRepository contentRepository,
		IMarkdownRenderer markdown,
		INewsStore newsStore,
		ITranslationService translation,
		ILanguageService language,
		IUiStringsService strings,
		IAiSettingsService aiSettings)
	{
		_contentRepository = contentRepository;
		_markdown = markdown;
		_newsStore = newsStore;
		_translation = translation;
		_language = language;
		_strings = strings;
		_aiSettings = aiSettings;

		TranslateCommand = new Command(async () => await TranslateAsync(), () => CanTranslate);
		AskQuestionCommand = new Command(async () => await AskQuestionAsync(), () => CanAsk);
		ToggleTranslationCommand = new Command(() => ShowTranslation = !ShowTranslation, () => ShowTranslationToggle);

		_language.PropertyChanged += async (_, e) =>
		{
			if (e.PropertyName != nameof(ILanguageService.CurrentLanguage))
				return;

			await RefreshTranslationStateAsync();
			if (ShowTranslation && HasTranslation)
				await ApplyViewModeAsync();
			else
				await RenderMarkdownAsync(_originalMarkdown, _originalTitle);

			NotifyLocalizedStringsChanged();
		};

		_strings.PropertyChanged += (_, _) => NotifyLocalizedStringsChanged();
	}

	private void NotifyLocalizedStringsChanged()
	{
		OnPropertyChanged(nameof(TranslationToggleLabel));
		OnPropertyChanged(nameof(TranslateLabel));
		OnPropertyChanged(nameof(AskQuestionLabel));
	}

	public async Task LoadAsync(string? title, string? bodyPath, string? articleId = null)
	{
		if (IsLoading) return;
		if (string.IsNullOrWhiteSpace(bodyPath)) return;

		IsLoading = true;
		try
		{
			_bodyPath = bodyPath;
			_articleId = articleId;
			_originalTitle = title ?? string.Empty;
			Title = _originalTitle;
			IsNewsArticle = bodyPath.StartsWith("appdata:", StringComparison.OrdinalIgnoreCase);
			ShowTranslation = false;

			_originalMarkdown = await _contentRepository.GetArticleMarkdownAsync(bodyPath);
			await RefreshTranslationStateAsync();
			await RenderMarkdownAsync(_originalMarkdown, _originalTitle);
		}
		finally
		{
			IsLoading = false;
		}
	}

	private async Task RefreshTranslationStateAsync()
	{
		if (!IsNewsArticle || string.IsNullOrWhiteSpace(_articleId))
		{
			HasTranslation = false;
			return;
		}

		var translation = await _newsStore.GetTranslationAsync(_articleId, _language.CurrentLanguage);
		HasTranslation = translation is not null;
	}

	private async Task ApplyViewModeAsync()
	{
		if (!ShowTranslation || !HasTranslation || string.IsNullOrWhiteSpace(_articleId))
		{
			await RenderMarkdownAsync(_originalMarkdown, _originalTitle);
			return;
		}

		var translation = await _newsStore.GetTranslationAsync(_articleId, _language.CurrentLanguage);
		if (translation is null)
		{
			ShowTranslation = false;
			return;
		}

		var md = await _contentRepository.GetArticleMarkdownAsync(translation.BodyPath);
		var body = ArticleMarkdownHelper.NormalizeTranslatedBody(md, translation.Title);
		await RenderMarkdownAsync(body, translation.Title ?? _originalTitle);
	}

	private async Task RenderMarkdownAsync(string markdown, string displayTitle)
	{
		Title = displayTitle;
		var html = WrapHtml(_markdown.ToHtml(markdown));
		HtmlSource = new HtmlWebViewSource { Html = html };
	}

	private async Task TranslateAsync()
	{
		if (IsBusy || !IsNewsArticle || string.IsNullOrWhiteSpace(_articleId))
			return;

		IsBusy = true;
		try
		{
			var lang = _language.CurrentLanguage;
			var existing = await _newsStore.GetTranslationAsync(_articleId, lang);
			if (existing is not null)
			{
				ShowTranslation = true;
				await ApplyViewModeAsync();
				return;
			}

			if (!await EnsureOnlineAsync())
				return;

			var bodyToTranslate = ArticleMarkdownHelper.StripLeadingHeading(_originalMarkdown);
			var result = await _translation.TranslateArticleAsync(
				_originalTitle,
				bodyToTranslate,
				sourceLang: "pl",
				targetLang: lang);

			var translatedBody = ArticleMarkdownHelper.NormalizeTranslatedBody(
				result.TranslatedMarkdown,
				result.TranslatedTitle);

			await _newsStore.SaveTranslationAsync(_articleId, lang, translatedBody, result.TranslatedTitle);
			HasTranslation = true;
			ShowTranslation = true;
		}
		catch (Exception ex)
		{
			var message = FormatErrorMessage(ex);
			await Shell.Current.DisplayAlertAsync(_strings.Error, message, _strings.Ok);
		}
		finally
		{
			IsBusy = false;
			((Command)TranslateCommand).ChangeCanExecute();
			((Command)AskQuestionCommand).ChangeCanExecute();
		}
	}

	private async Task AskQuestionAsync()
	{
		if (string.IsNullOrWhiteSpace(_articleId))
			return;

		var route =
			$"Chat?articleId={Uri.EscapeDataString(_articleId)}&bodyPath={Uri.EscapeDataString(_bodyPath)}&title={Uri.EscapeDataString(_originalTitle)}";
		await Shell.Current.GoToAsync(route);
	}

	private async Task<bool> EnsureOnlineAsync()
	{
		if (_aiSettings.StubMode)
			return true;

		if (Connectivity.NetworkAccess == NetworkAccess.Internet)
			return true;

		await Shell.Current.DisplayAlertAsync(_strings.Error, _strings.NoInternet, _strings.Ok);
		return false;
	}

	private static string WrapHtml(string bodyHtml)
	{
		const string prefix = @"<!doctype html>
<html>
  <head>
    <meta name=""viewport"" content=""width=device-width, initial-scale=1"" />
    <style>
      :root {
        --bg: #ffffff;
        --fg: #1b1b1b;
        --muted: #5a5a5a;
        --link: #d96b4a;
        --codebg: #f4f4f4;
      }
      @media (prefers-color-scheme: dark) {
        :root {
          --bg: #121212;
          --fg: #f1f1f1;
          --muted: #b7b7b7;
          --link: #ff8a65;
          --codebg: #1e1e1e;
        }
      }
      body {
        margin: 0;
        padding: 16px;
        background: var(--bg);
        color: var(--fg);
        font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, Arial, sans-serif;
        line-height: 1.45;
      }
      h1, h2, h3 { line-height: 1.15; }
      a { color: var(--link); }
      code, pre {
        font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, ""Liberation Mono"", ""Courier New"", monospace;
      }
      pre {
        background: var(--codebg);
        padding: 12px;
        border-radius: 8px;
        overflow-x: auto;
      }
      blockquote {
        margin: 12px 0;
        padding: 0 12px;
        border-left: 3px solid var(--link);
        color: var(--muted);
      }
      ul { padding-left: 22px; }
    </style>
  </head>
  <body>";

		const string suffix = @"  </body>
</html>";

		return prefix + bodyHtml + suffix;
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

		return msg.Contains("connection failure", StringComparison.OrdinalIgnoreCase)
			|| msg.Contains("failed to connect", StringComparison.OrdinalIgnoreCase)
			|| msg.Contains("network is unreachable", StringComparison.OrdinalIgnoreCase);
	}

	private void OnPropertyChanged([CallerMemberName] string? name = null)
		=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
