using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using AngleSharp;

namespace Himi.Services;

public sealed record ParsedNewsListItem(
	string Id,
	string Title,
	DateTime PublishedUtc,
	string Url,
	string? Intro,
	string BodyPath);

public interface INewsSource
{
	string SourceId { get; }
	Task<IReadOnlyList<ParsedNewsListItem>> FetchLatestAsync(int take, CancellationToken cancellationToken = default);
	Task<string> DownloadAsMarkdownAsync(string url, CancellationToken cancellationToken = default);
}

/// <summary>
/// gov.pl (UA section) news list + article extractor.
/// Trade-off: this is HTML parsing and can break if site markup changes.
/// </summary>
public sealed class GovPlUaNewsSource : INewsSource
{
	private static readonly Uri BaseUri = new("https://www.gov.pl");
	private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

	private readonly HttpClient _http;
	private readonly IBrowsingContext _ctx;
	private readonly INewsStore _store;

	public string SourceId => "govpl-ua-novyny";

	public GovPlUaNewsSource(HttpClient http, INewsStore store)
	{
		_http = http;
		_store = store;
		_ctx = BrowsingContext.New(Configuration.Default);
	}

	public async Task<IReadOnlyList<ParsedNewsListItem>> FetchLatestAsync(int take, CancellationToken cancellationToken = default)
	{
		var listUrl = new Uri(BaseUri, "/web/ua/Novyny");
		var html = await _http.GetStringAsync(listUrl, cancellationToken);
		var doc = await _ctx.OpenAsync(req => req.Content(html), cancellationToken);

		var items = new List<ParsedNewsListItem>();
		foreach (var li in doc.QuerySelectorAll("li"))
		{
			var dateText = li.QuerySelector(".event .date")?.TextContent?.Trim();
			var a = li.QuerySelector(".title a");
			if (string.IsNullOrWhiteSpace(dateText) || a is null)
				continue;

			if (!DateTime.TryParseExact(dateText, "dd.MM.yyyy", Invariant, DateTimeStyles.AssumeUniversal, out var published))
				continue;

			var href = a.GetAttribute("href")?.Trim();
			if (string.IsNullOrWhiteSpace(href))
				continue;

			var url = new Uri(BaseUri, href).ToString();
			var title = a.TextContent?.Trim();
			if (string.IsNullOrWhiteSpace(title))
				continue;

			var intro = li.QuerySelector(".intro")?.TextContent?.Trim();

			// Use a short stable id (hash of canonical URL).
			var id = MakeId(url, published);
			var bodyPath = _store.GetArticleBodyPath(id);

			items.Add(new ParsedNewsListItem(
				Id: id,
				Title: NormalizeSpaces(title),
				PublishedUtc: published.ToUniversalTime(),
				Url: url,
				Intro: string.IsNullOrWhiteSpace(intro) ? null : NormalizeSpaces(intro),
				BodyPath: bodyPath));
		}

		return items
			.OrderByDescending(i => i.PublishedUtc)
			.DistinctBy(i => i.Url)
			.Take(take)
			.ToList();
	}

	public async Task<string> DownloadAsMarkdownAsync(string url, CancellationToken cancellationToken = default)
	{
		var html = await _http.GetStringAsync(url, cancellationToken);
		var doc = await _ctx.OpenAsync(req => req.Content(html), cancellationToken);

		var title = doc.QuerySelector("#main-content h2")?.TextContent?.Trim()
			?? doc.Title?.Trim()
			?? "News";

		var dateText = doc.QuerySelector("#main-content .event-date")?.TextContent?.Trim();
		var publishedLabel = string.IsNullOrWhiteSpace(dateText) ? null : dateText;

		var contentRoot = doc.QuerySelector("#main-content .editor-content") ?? doc.QuerySelector(".editor-content");
		var body = contentRoot is null
			? "(Failed to extract article body.)"
			: ToMarkdown(contentRoot, BaseUri);

		var sb = new StringBuilder();
		sb.AppendLine("# " + NormalizeSpaces(title));
		sb.AppendLine();
		if (!string.IsNullOrWhiteSpace(publishedLabel))
		{
			sb.AppendLine($"_Дата: {publishedLabel}_");
			sb.AppendLine();
		}
		sb.AppendLine(body.Trim());
		sb.AppendLine();
		sb.AppendLine($"Источник: [{url}]({url})");
		sb.AppendLine();

		return sb.ToString();
	}

	private static string MakeId(string url, DateTime publishedUtc)
	{
		var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(url));
		var hash8 = Convert.ToHexString(bytes).ToLowerInvariant()[..8];
		return $"govpl_{publishedUtc:yyyyMMdd}_{hash8}";
	}

	private static string NormalizeSpaces(string s)
		=> string.Join(" ", s.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));

	private static string ToMarkdown(AngleSharp.Dom.IElement root, Uri baseUri)
	{
		var sb = new StringBuilder();
		foreach (var node in root.ChildNodes)
			RenderBlock(node, sb, baseUri);
		return sb.ToString();
	}

	private static void RenderBlock(AngleSharp.Dom.INode node, StringBuilder sb, Uri baseUri)
	{
		if (node is AngleSharp.Dom.IText text)
		{
			var t = NormalizeSpaces(text.Text);
			if (!string.IsNullOrWhiteSpace(t))
				sb.AppendLine(t);
			return;
		}

		if (node is not AngleSharp.Dom.IElement el)
			return;

		switch (el.TagName.ToLowerInvariant())
		{
			case "h2":
			case "h3":
			case "h4":
				sb.AppendLine("## " + NormalizeSpaces(RenderInline(el, baseUri)));
				sb.AppendLine();
				return;
			case "p":
				{
					var p = NormalizeSpaces(RenderInline(el, baseUri));
					if (!string.IsNullOrWhiteSpace(p) && p != "\u00a0")
					{
						sb.AppendLine(p);
						sb.AppendLine();
					}
					return;
				}
			case "ul":
				foreach (var li in el.QuerySelectorAll("li"))
				{
					var item = NormalizeSpaces(RenderInline(li, baseUri));
					if (!string.IsNullOrWhiteSpace(item))
						sb.AppendLine("- " + item);
				}
				sb.AppendLine();
				return;
			case "ol":
				{
					var i = 1;
					foreach (var li in el.QuerySelectorAll("li"))
					{
						var item = NormalizeSpaces(RenderInline(li, baseUri));
						if (!string.IsNullOrWhiteSpace(item))
							sb.AppendLine($"{i++}. {item}");
					}
					sb.AppendLine();
					return;
				}
			case "div":
			case "section":
			case "span":
				foreach (var child in el.ChildNodes)
					RenderBlock(child, sb, baseUri);
				return;
			default:
				// Best-effort: keep traversing.
				foreach (var child in el.ChildNodes)
					RenderBlock(child, sb, baseUri);
				return;
		}
	}

	private static string RenderInline(AngleSharp.Dom.IElement el, Uri baseUri)
	{
		var sb = new StringBuilder();
		foreach (var node in el.ChildNodes)
			RenderInlineNode(node, sb, baseUri);
		return sb.ToString();
	}

	private static void RenderInlineNode(AngleSharp.Dom.INode node, StringBuilder sb, Uri baseUri)
	{
		if (node is AngleSharp.Dom.IText t)
		{
			sb.Append(t.Text);
			return;
		}

		if (node is not AngleSharp.Dom.IElement el)
			return;

		var tag = el.TagName.ToLowerInvariant();
		switch (tag)
		{
			case "br":
				sb.AppendLine();
				return;
			case "strong":
			case "b":
				sb.Append("**");
				sb.Append(RenderInline(el, baseUri));
				sb.Append("**");
				return;
			case "em":
			case "i":
				sb.Append("*");
				sb.Append(RenderInline(el, baseUri));
				sb.Append("*");
				return;
			case "a":
				{
					var href = el.GetAttribute("href")?.Trim();
					var text = NormalizeSpaces(RenderInline(el, baseUri));
					if (string.IsNullOrWhiteSpace(href) || string.IsNullOrWhiteSpace(text))
					{
						sb.Append(text);
						return;
					}

					var abs = new Uri(baseUri, href).ToString();
					sb.Append('[').Append(text).Append(']');
					sb.Append('(').Append(abs).Append(')');
					return;
				}
			default:
				sb.Append(RenderInline(el, baseUri));
				return;
		}
	}
}

