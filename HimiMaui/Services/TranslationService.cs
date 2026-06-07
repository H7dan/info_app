using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Himi.Services;

public interface ITranslationService
{
	Task<TranslationResult> TranslateArticleAsync(string title, string markdown, string sourceLang, string targetLang, CancellationToken cancellationToken = default);
}

public sealed record TranslationResult(string TranslatedTitle, string TranslatedMarkdown);

public sealed class TranslationService : ITranslationService
{
	private readonly HttpClient _http;
	private readonly IAiSettingsService _settings;

	public TranslationService(HttpClient http, IAiSettingsService settings)
	{
		_http = http;
		_settings = settings;
	}

	public async Task<TranslationResult> TranslateArticleAsync(string title, string markdown, string sourceLang, string targetLang, CancellationToken cancellationToken = default)
	{
		if (_settings.StubMode)
		{
			return new TranslationResult(
				$"[Stub] {title}",
				$"[Stub translation {sourceLang}->{targetLang}]\n\n{markdown}");
		}

		var payload = new TranslateRequest(title, markdown, sourceLang, targetLang);
		var response = await _http.PostAsJsonAsync($"{_settings.BaseUrl}/v1/translate", payload, cancellationToken);
		response.EnsureSuccessStatusCode();

		var body = await response.Content.ReadFromJsonAsync<TranslateResponseDto>(cancellationToken: cancellationToken);
		return new TranslationResult(body?.TranslatedTitle ?? title, body?.TranslatedMarkdown ?? string.Empty);
	}

	private sealed record TranslateRequest(
		[property: JsonPropertyName("title")] string Title,
		[property: JsonPropertyName("markdown")] string Markdown,
		[property: JsonPropertyName("sourceLang")] string SourceLang,
		[property: JsonPropertyName("targetLang")] string TargetLang);

	private sealed record TranslateResponseDto(
		[property: JsonPropertyName("translatedTitle")] string TranslatedTitle,
		[property: JsonPropertyName("translatedMarkdown")] string TranslatedMarkdown);
}
