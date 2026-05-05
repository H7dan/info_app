using System.Text.Json.Serialization;

namespace Himi.Models;

public sealed record NewsIndex(
	[property: JsonPropertyName("schemaVersion")] int SchemaVersion,
	[property: JsonPropertyName("items")] IReadOnlyList<NewsItem> Items);

public sealed record NewsItem(
	[property: JsonPropertyName("id")] string Id,
	[property: JsonPropertyName("sourceId")] string SourceId,
	[property: JsonPropertyName("title")] string Title,
	[property: JsonPropertyName("publishedUtc")] DateTime PublishedUtc,
	[property: JsonPropertyName("url")] string Url,
	[property: JsonPropertyName("intro")] string? Intro,
	[property: JsonPropertyName("bodyPath")] string BodyPath);

