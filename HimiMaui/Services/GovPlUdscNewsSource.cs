using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using AngleSharp;

namespace Himi.Services;

/// <summary>
/// gov.pl UdSC (Urząd do Spraw Cudzoziemców) news list + article extractor.
/// Trade-off: this is HTML parsing and can break if site markup changes.
/// </summary>
public sealed class GovPlUdscNewsSource : INewsSource
{
	private static readonly Uri BaseUri = new("https://www.gov.pl");
	private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

	private readonly HttpClient _http;
	private readonly IBrowsingContext _ctx;
	private readonly INewsStore _store;

	public string SourceId => "govpl-udsc";

	public GovPlUdscNewsSource(HttpClient http, INewsStore store)
	{
		_http = http;
		_store = store;
		_ctx = BrowsingContext.New(Configuration.Default);
	}

	public async Task<IReadOnlyList<ParsedNewsListItem>> FetchLatestAsync(int take, CancellationToken cancellationToken = default)
	{
		var listUrl = new Uri(BaseUri, "/web/udsc/urzad-do-spraw-cudzoziemcow");
		var html = await _http.GetStringAsync(listUrl, cancellationToken);
		var doc = await _ctx.OpenAsync(req => req.Content(html), cancellationToken);

		// The page contains a dedicated news section:
		// <section id="Aktualnosci" ...> ... <span class="date">dd.MM.yyyy</span> ... <div class="title"><a href="...">...</a>
		var section = doc.QuerySelector("section#Aktualnosci") ?? doc.QuerySelector("#Aktualnosci");

		var items = new List<ParsedNewsListItem>();
		foreach (var li in (section?.QuerySelectorAll("li") ?? doc.QuerySelectorAll("section#Aktualnosci li")))
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

			var id = MakeId(url, published);
			var bodyPath = _store.GetArticleBodyPath(id);

			items.Add(new ParsedNewsListItem(
				Id: id,
				Title: NormalizeSpaces(title),
				PublishedUtc: published.ToUniversalTime(),
				Url: url,
				Intro: null,
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
		var html = await _http.GetStringAsync(new Uri(url), cancellationToken);
		var doc = await _ctx.OpenAsync(req => req.Content(html), cancellationToken);

		var title = doc.QuerySelector("#main-content h2")?.TextContent?.Trim()
			?? doc.Title?.Trim()
			?? "News";

		var dateText = doc.QuerySelector("#main-content .event-date")?.TextContent?.Trim();
		var publishedLabel = string.IsNullOrWhiteSpace(dateText) ? null : dateText;

		var contentRoot = doc.QuerySelector("#main-content .editor-content") ?? doc.QuerySelector(".editor-content");
		var body = contentRoot is null
			? "(Failed to extract article body.)"
			: GovPlMarkdown.ToMarkdown(contentRoot, BaseUri);

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
		return $"udsc_{publishedUtc:yyyyMMdd}_{hash8}";
	}

	private static string NormalizeSpaces(string s)
		=> string.Join(" ", s.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));
}

internal static class GovPlMarkdown
{
	internal static string ToMarkdown(AngleSharp.Dom.IElement root, Uri baseUri)
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

		switch (el.TagName.ToLowerInvariant())
		{
			case "br":
				sb.AppendLine();
				return;
			case "strong":
			case "b":
				sb.Append("**").Append(RenderInline(el, baseUri)).Append("**");
				return;
			case "em":
			case "i":
				sb.Append("*").Append(RenderInline(el, baseUri)).Append("*");
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
					sb.Append('[').Append(text).Append(']').Append('(').Append(abs).Append(')');
					return;
				}
			default:
				sb.Append(RenderInline(el, baseUri));
				return;
		}
	}

	private static string NormalizeSpaces(string s)
		=> string.Join(" ", s.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));
}

