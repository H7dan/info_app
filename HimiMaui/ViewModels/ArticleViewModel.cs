using System.ComponentModel;
using System.Runtime.CompilerServices;
using Himi.Services;

namespace Himi.ViewModels;

public sealed class ArticleViewModel : INotifyPropertyChanged
{
	private readonly IContentRepository _contentRepository;
	private readonly IMarkdownRenderer _markdown;

	private string _title = string.Empty;
	private HtmlWebViewSource _htmlSource = new();
	private bool _isLoading;

	public event PropertyChangedEventHandler? PropertyChanged;

	public string Title
	{
		get => _title;
		private set
		{
			if (_title == value) return;
			_title = value;
			OnPropertyChanged();
		}
	}

	public HtmlWebViewSource HtmlSource
	{
		get => _htmlSource;
		private set
		{
			if (ReferenceEquals(_htmlSource, value)) return;
			_htmlSource = value;
			OnPropertyChanged();
		}
	}

	public bool IsLoading
	{
		get => _isLoading;
		private set
		{
			if (_isLoading == value) return;
			_isLoading = value;
			OnPropertyChanged();
		}
	}

	public ArticleViewModel(IContentRepository contentRepository, IMarkdownRenderer markdown)
	{
		_contentRepository = contentRepository;
		_markdown = markdown;
	}

	public async Task LoadAsync(string? title, string? bodyPath)
	{
		if (IsLoading) return;
		if (string.IsNullOrWhiteSpace(bodyPath)) return;

		IsLoading = true;
		try
		{
			Title = title ?? string.Empty;

			var md = await _contentRepository.GetArticleMarkdownAsync(bodyPath);
			var html = WrapHtml(_markdown.ToHtml(md));
			HtmlSource = new HtmlWebViewSource { Html = html };
		}
		finally
		{
			IsLoading = false;
		}
	}

	private static string WrapHtml(string bodyHtml)
	{
		// Keep CSS self-contained: works offline, handles light/dark via media query.
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

	private void OnPropertyChanged([CallerMemberName] string? name = null)
		=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

