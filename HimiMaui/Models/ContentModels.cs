using System.Text.Json.Serialization;

namespace Himi.Models;

public sealed record ContentIndex(
	[property: JsonPropertyName("schemaVersion")] int SchemaVersion,
	[property: JsonPropertyName("contentVersion")] string ContentVersion,
	[property: JsonPropertyName("generatedAtUtc")] DateTime GeneratedAtUtc,
	[property: JsonPropertyName("categories")] IReadOnlyList<Category> Categories,
	[property: JsonPropertyName("articles")] IReadOnlyList<Article> Articles,
	[property: JsonPropertyName("checklists")] IReadOnlyList<Checklist> Checklists,
	[property: JsonPropertyName("contacts")] IReadOnlyList<PhoneContact> Contacts
);

public sealed record Category(
	[property: JsonPropertyName("id")] string Id,
	[property: JsonPropertyName("title")] string Title,
	[property: JsonPropertyName("sort")] int Sort
);

public sealed record Article(
	[property: JsonPropertyName("id")] string Id,
	[property: JsonPropertyName("lang")] string Lang,
	[property: JsonPropertyName("title")] string Title,
	[property: JsonPropertyName("summary")] string Summary,
	[property: JsonPropertyName("categoryId")] string CategoryId,
	[property: JsonPropertyName("tags")] IReadOnlyList<string> Tags,
	[property: JsonPropertyName("bodyPath")] string BodyPath,
	[property: JsonPropertyName("relatedIds")] IReadOnlyList<string> RelatedIds,
	[property: JsonPropertyName("sources")] IReadOnlyList<string>? Sources
);

public sealed record Checklist(
	[property: JsonPropertyName("id")] string Id,
	[property: JsonPropertyName("lang")] string Lang,
	[property: JsonPropertyName("title")] string Title,
	[property: JsonPropertyName("sections")] IReadOnlyList<ChecklistSection> Sections
);

public sealed record ChecklistSection(
	[property: JsonPropertyName("id")] string Id,
	[property: JsonPropertyName("title")] string Title,
	[property: JsonPropertyName("items")] IReadOnlyList<ChecklistItem> Items
);

public sealed record ChecklistItem(
	[property: JsonPropertyName("id")] string Id,
	[property: JsonPropertyName("text")] string Text,
	[property: JsonPropertyName("linksToArticleId")] string? LinksToArticleId
);

public sealed record PhoneContact(
	[property: JsonPropertyName("id")] string Id,
	[property: JsonPropertyName("lang")] string Lang,
	[property: JsonPropertyName("title")] string Title,
	[property: JsonPropertyName("number")] string Number,
	[property: JsonPropertyName("category")] string Category,
	[property: JsonPropertyName("note")] string Note
);

