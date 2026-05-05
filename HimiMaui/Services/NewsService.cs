using Himi.Models;

namespace Himi.Services;

public interface INewsService
{
	Task<IReadOnlyList<NewsItem>> GetCachedAsync(CancellationToken cancellationToken = default);
	Task<IReadOnlyList<NewsItem>> RefreshAsync(CancellationToken cancellationToken = default);
}

public sealed class NewsService : INewsService
{
	private const int FetchTakePerSource = 10;
	private const int MaxStored = 200;

	private readonly INewsStore _store;
	private readonly IReadOnlyList<INewsSource> _sources;

	public NewsService(INewsStore store, IEnumerable<INewsSource> sources)
	{
		_store = store;
		_sources = sources.ToList();
	}

	public async Task<IReadOnlyList<NewsItem>> GetCachedAsync(CancellationToken cancellationToken = default)
	{
		var index = await _store.LoadIndexAsync(cancellationToken);
		return index.Items.OrderByDescending(i => i.PublishedUtc).ToList();
	}

	public async Task<IReadOnlyList<NewsItem>> RefreshAsync(CancellationToken cancellationToken = default)
	{
		var index = await _store.LoadIndexAsync(cancellationToken);
		var byUrl = index.Items.ToDictionary(i => i.Url, StringComparer.OrdinalIgnoreCase);

		var newItems = new List<NewsItem>();

		foreach (var source in _sources)
		{
			var latest = await source.FetchLatestAsync(FetchTakePerSource, cancellationToken);
			foreach (var it in latest)
			{
				if (byUrl.ContainsKey(it.Url))
					continue;

				var md = await source.DownloadAsMarkdownAsync(it.Url, cancellationToken);
				await _store.SaveArticleMarkdownAsync(it.Id, md, cancellationToken);

				newItems.Add(new NewsItem(
					Id: it.Id,
					SourceId: source.SourceId,
					Title: it.Title,
					PublishedUtc: it.PublishedUtc,
					Url: it.Url,
					Intro: it.Intro,
					BodyPath: it.BodyPath));

				byUrl[it.Url] = newItems[^1];
			}
		}

		var merged = index.Items.Concat(newItems)
			.OrderByDescending(i => i.PublishedUtc)
			.GroupBy(i => i.Url, StringComparer.OrdinalIgnoreCase)
			.Select(g => g.First())
			.Take(MaxStored)
			.ToList();

		// Cleanup markdown files for trimmed-out items.
		var keptIds = merged.Select(i => i.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
		var removed = index.Items.Where(i => !keptIds.Contains(i.Id)).Select(i => i.Id).Distinct(StringComparer.OrdinalIgnoreCase);
		foreach (var id in removed)
			await _store.DeleteArticleAsync(id, cancellationToken);

		var saved = new NewsIndex(1, merged);
		await _store.SaveIndexAsync(saved, cancellationToken);

		return merged;
	}
}

