using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Himi.Services;

public interface IAssistantService
{
	Task<string> ChatAsync(string message, string language, IReadOnlyList<ChatTurn> history, CancellationToken cancellationToken = default);
	Task<string> ChatAboutArticleAsync(string message, string language, string articleTitle, string articleMarkdown, IReadOnlyList<ChatTurn> history, CancellationToken cancellationToken = default);
	Task<bool> IsServerAvailableAsync(CancellationToken cancellationToken = default);
}

public sealed record ChatTurn(string Role, string Content);

public sealed class AssistantService : IAssistantService
{
	private readonly HttpClient _http;
	private readonly IAiSettingsService _settings;

	public AssistantService(HttpClient http, IAiSettingsService settings)
	{
		_http = http;
		_settings = settings;
	}

	public async Task<bool> IsServerAvailableAsync(CancellationToken cancellationToken = default)
	{
		if (_settings.StubMode)
			return true;

		try
		{
			var response = await _http.GetAsync($"{_settings.BaseUrl}/health", cancellationToken);
			return response.IsSuccessStatusCode;
		}
		catch
		{
			return false;
		}
	}

	public Task<string> ChatAsync(string message, string language, IReadOnlyList<ChatTurn> history, CancellationToken cancellationToken = default)
	{
		if (_settings.StubMode)
			return Task.FromResult($"[Stub] {message}");

		var payload = new ChatRequest(message, language, history.Select(h => new HistoryDto(h.Role, h.Content)).ToList());
		return PostForReplyAsync($"{_settings.BaseUrl}/v1/chat", payload, cancellationToken);
	}

	public Task<string> ChatAboutArticleAsync(string message, string language, string articleTitle, string articleMarkdown, IReadOnlyList<ChatTurn> history, CancellationToken cancellationToken = default)
	{
		if (_settings.StubMode)
			return Task.FromResult($"[Stub] {message}");

		var payload = new ChatAboutArticleRequest(
			message,
			language,
			articleTitle,
			articleMarkdown,
			history.Select(h => new HistoryDto(h.Role, h.Content)).ToList());

		return PostForReplyAsync($"{_settings.BaseUrl}/v1/chat-about-article", payload, cancellationToken);
	}

	private async Task<string> PostForReplyAsync<T>(string url, T payload, CancellationToken cancellationToken)
	{
		var response = await _http.PostAsJsonAsync(url, payload, cancellationToken);
		response.EnsureSuccessStatusCode();
		var body = await response.Content.ReadFromJsonAsync<ChatResponseDto>(cancellationToken: cancellationToken);
		return body?.Reply ?? string.Empty;
	}

	private sealed record HistoryDto(
		[property: JsonPropertyName("role")] string Role,
		[property: JsonPropertyName("content")] string Content);

	private sealed record ChatRequest(
		[property: JsonPropertyName("message")] string Message,
		[property: JsonPropertyName("language")] string Language,
		[property: JsonPropertyName("history")] IReadOnlyList<HistoryDto> History);

	private sealed record ChatAboutArticleRequest(
		[property: JsonPropertyName("message")] string Message,
		[property: JsonPropertyName("language")] string Language,
		[property: JsonPropertyName("articleTitle")] string ArticleTitle,
		[property: JsonPropertyName("articleMarkdown")] string ArticleMarkdown,
		[property: JsonPropertyName("history")] IReadOnlyList<HistoryDto> History);

	private sealed record ChatResponseDto([property: JsonPropertyName("reply")] string Reply);
}
