using System.Text.Json;
using Himi.Models;

namespace Himi.Services;

public interface IContentRepository
{
	Task<ContentIndex> GetIndexAsync(CancellationToken cancellationToken = default);
	Task<string> GetArticleMarkdownAsync(string bodyPath, CancellationToken cancellationToken = default);
}

public sealed class ContentRepository : IContentRepository
{
	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		PropertyNameCaseInsensitive = true
	};

	private ContentIndex? _cachedIndex;

	public async Task<ContentIndex> GetIndexAsync(CancellationToken cancellationToken = default)
	{
		if (_cachedIndex is not null)
			return _cachedIndex;

		await using var stream = await FileSystem.OpenAppPackageFileAsync("Content/index.json");
		using var reader = new StreamReader(stream);
		var json = await reader.ReadToEndAsync(cancellationToken);

		var index = JsonSerializer.Deserialize<ContentIndex>(json, JsonOptions)
			?? throw new InvalidOperationException("Failed to deserialize Content/index.json");

		_cachedIndex = index;
		return index;
	}

	public async Task<string> GetArticleMarkdownAsync(string bodyPath, CancellationToken cancellationToken = default)
	{
		// `bodyPath` is a logical path inside the app package, e.g. "Content/articles/ua/emergency_numbers.md".
		await using var stream = await FileSystem.OpenAppPackageFileAsync(bodyPath);
		using var reader = new StreamReader(stream);
		return await reader.ReadToEndAsync(cancellationToken);
	}
}

