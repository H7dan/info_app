using System.Text.Json.Serialization;

namespace Himi.Models;

public sealed record ChatMessage(
	[property: JsonPropertyName("role")] string Role,
	[property: JsonPropertyName("content")] string Content,
	[property: JsonPropertyName("sentUtc")] DateTime SentUtc);

public sealed record ChatHistory(
	[property: JsonPropertyName("schemaVersion")] int SchemaVersion,
	[property: JsonPropertyName("messages")] IReadOnlyList<ChatMessage> Messages);
