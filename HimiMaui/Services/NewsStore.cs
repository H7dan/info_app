using System.Text.Json;
using Himi.Models;

namespace Himi.Services;

public interface INewsStore
{
	Task<NewsIndex> LoadIndexAsync(CancellationToken cancellationToken = default);
	Task SaveIndexAsync(NewsIndex index, CancellationToken cancellationToken = default);
	Task SaveArticleMarkdownAsync(string articleId, string markdown, CancellationToken cancellationToken = default);
	Task DeleteArticleAsync(string articleId, CancellationToken cancellationToken = default);
	string GetArticleBodyPath(string articleId);
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
	private static string IndexPath => Path.Combine(RootDir, "index.json");

	public async Task<NewsIndex> LoadIndexAsync(CancellationToken cancellationToken = default)
	{
		Directory.CreateDirectory(RootDir);
		Directory.CreateDirectory(ArticlesDir);

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

		var json = JsonSerializer.Serialize(index, JsonOptions);
		await File.WriteAllTextAsync(IndexPath, json, cancellationToken);
	}

	public async Task SaveArticleMarkdownAsync(string articleId, string markdown, CancellationToken cancellationToken = default)
	{
		Directory.CreateDirectory(ArticlesDir);
		var path = Path.Combine(ArticlesDir, $"{articleId}.md");
		await File.WriteAllTextAsync(path, markdown, cancellationToken);
	}

	public Task DeleteArticleAsync(string articleId, CancellationToken cancellationToken = default)
	{
		var path = Path.Combine(ArticlesDir, $"{articleId}.md");
		if (File.Exists(path))
			File.Delete(path);
		return Task.CompletedTask;
	}

	public string GetArticleBodyPath(string articleId)
		=> $"appdata:News/articles/{articleId}.md";
}

