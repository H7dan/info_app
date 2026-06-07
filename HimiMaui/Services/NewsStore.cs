using System.Text.Json;
using Himi.Models;

namespace Himi.Services;

public interface INewsStore
{
	Task<NewsIndex> LoadIndexAsync(CancellationToken cancellationToken = default);
	Task SaveIndexAsync(NewsIndex index, CancellationToken cancellationToken = default);
	Task SaveArticleMarkdownAsync(string articleId, string markdown, CancellationToken cancellationToken = default);
	Task DeleteArticleAsync(string articleId, CancellationToken cancellationToken = default);
	Task<NewsTranslation?> GetTranslationAsync(string articleId, string lang, CancellationToken cancellationToken = default);
	Task SaveTranslationAsync(string articleId, string lang, string markdown, string? translatedTitle, CancellationToken cancellationToken = default);
	Task DeleteTranslationsForArticleAsync(string articleId, CancellationToken cancellationToken = default);
	string GetArticleBodyPath(string articleId);
	string GetTranslationBodyPath(string articleId, string lang);
}

public sealed class NewsStore : INewsStore
{
	private const int SchemaVersion = 1;
	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		PropertyNameCaseInsensitive = true,
		WriteIndented = true
	};

	private static string RootDir => Path.Combine(FileSystem.AppDataDirectory, "News");
	private static string ArticlesDir => Path.Combine(RootDir, "articles");
	private static string TranslationsDir => Path.Combine(RootDir, "translations");
	private static string IndexPath => Path.Combine(RootDir, "index.json");

	public async Task<NewsIndex> LoadIndexAsync(CancellationToken cancellationToken = default)
	{
		Directory.CreateDirectory(RootDir);
		Directory.CreateDirectory(ArticlesDir);
		Directory.CreateDirectory(TranslationsDir);

		if (!File.Exists(IndexPath))
			return new NewsIndex(SchemaVersion, Array.Empty<NewsItem>());

		var json = await File.ReadAllTextAsync(IndexPath, cancellationToken);
		return JsonSerializer.Deserialize<NewsIndex>(json, JsonOptions)
			?? new NewsIndex(SchemaVersion, Array.Empty<NewsItem>());
	}

	public async Task SaveIndexAsync(NewsIndex index, CancellationToken cancellationToken = default)
	{
		Directory.CreateDirectory(RootDir);
		Directory.CreateDirectory(ArticlesDir);
		Directory.CreateDirectory(TranslationsDir);

		var json = JsonSerializer.Serialize(index, JsonOptions);
		await File.WriteAllTextAsync(IndexPath, json, cancellationToken);
	}

	public async Task SaveArticleMarkdownAsync(string articleId, string markdown, CancellationToken cancellationToken = default)
	{
		Directory.CreateDirectory(ArticlesDir);
		var path = Path.Combine(ArticlesDir, $"{articleId}.md");
		await File.WriteAllTextAsync(path, markdown, cancellationToken);
	}

	public async Task DeleteArticleAsync(string articleId, CancellationToken cancellationToken = default)
	{
		var path = Path.Combine(ArticlesDir, $"{articleId}.md");
		if (File.Exists(path))
			File.Delete(path);

		await DeleteTranslationsForArticleAsync(articleId, cancellationToken);
	}

	public async Task<NewsTranslation?> GetTranslationAsync(string articleId, string lang, CancellationToken cancellationToken = default)
	{
		var index = await LoadIndexAsync(cancellationToken);
		var item = index.Items.FirstOrDefault(i => string.Equals(i.Id, articleId, StringComparison.OrdinalIgnoreCase));
		return item?.Translations?.FirstOrDefault(t => string.Equals(t.Lang, lang, StringComparison.OrdinalIgnoreCase));
	}

	public async Task SaveTranslationAsync(string articleId, string lang, string markdown, string? translatedTitle, CancellationToken cancellationToken = default)
	{
		Directory.CreateDirectory(TranslationsDir);
		var path = Path.Combine(TranslationsDir, $"{articleId}_{lang}.md");
		await File.WriteAllTextAsync(path, markdown, cancellationToken);

		var index = await LoadIndexAsync(cancellationToken);
		var items = index.Items.ToList();
		var idx = items.FindIndex(i => string.Equals(i.Id, articleId, StringComparison.OrdinalIgnoreCase));
		if (idx < 0)
			return;

		var item = items[idx];
		var translations = (item.Translations ?? Array.Empty<NewsTranslation>()).ToList();
		var tIdx = translations.FindIndex(t => string.Equals(t.Lang, lang, StringComparison.OrdinalIgnoreCase));
		var entry = new NewsTranslation(lang, GetTranslationBodyPath(articleId, lang), translatedTitle);

		if (tIdx >= 0)
			translations[tIdx] = entry;
		else
			translations.Add(entry);

		items[idx] = item with { Translations = translations };
		await SaveIndexAsync(new NewsIndex(SchemaVersion, items), cancellationToken);
	}

	public Task DeleteTranslationsForArticleAsync(string articleId, CancellationToken cancellationToken = default)
	{
		if (Directory.Exists(TranslationsDir))
		{
			foreach (var file in Directory.GetFiles(TranslationsDir, $"{articleId}_*.md"))
				File.Delete(file);
		}

		return Task.CompletedTask;
	}

	public string GetArticleBodyPath(string articleId)
		=> $"appdata:News/articles/{articleId}.md";

	public string GetTranslationBodyPath(string articleId, string lang)
		=> $"appdata:News/translations/{articleId}_{lang}.md";
}
