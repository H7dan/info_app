using Microsoft.Extensions.Logging;
using Himi.Services;
using Himi.ViewModels;
using Himi.Pages;

namespace Himi;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		// Offline-first MVP: keep dependencies small and push state into simple services.
		builder.Services.AddSingleton<IContentRepository, ContentRepository>();
		builder.Services.AddSingleton<ILanguageService, LanguageService>();
		builder.Services.AddSingleton<IThemeService, ThemeService>();
		builder.Services.AddSingleton<IUiStringsService, UiStringsService>();
		builder.Services.AddSingleton<IMarkdownRenderer, MarkdownRenderer>();
		builder.Services.AddSingleton<IChecklistProgressStore, ChecklistProgressStore>();
		builder.Services.AddSingleton<INewsStore, NewsStore>();
		builder.Services.AddSingleton(sp =>
		{
			var http = new HttpClient
			{
				Timeout = TimeSpan.FromSeconds(25)
			};
			http.DefaultRequestHeaders.UserAgent.ParseAdd("HimiMaui/1.0 (+offline-first)");
			return http;
		});
		builder.Services.AddSingleton<INewsSource, GovPlUaNewsSource>();
		builder.Services.AddSingleton<INewsSource, GovPlUdscNewsSource>();
		builder.Services.AddSingleton<INewsService, NewsService>();
		builder.Services.AddSingleton<IAiSettingsService, AiSettingsService>();
		builder.Services.AddSingleton<IChatHistoryStore, ChatHistoryStore>();
		builder.Services.AddSingleton<IAssistantService>(sp =>
		{
			var http = CreateAiHttpClient();
			return new AssistantService(http, sp.GetRequiredService<IAiSettingsService>());
		});
		builder.Services.AddSingleton<ITranslationService>(sp =>
		{
			var http = CreateAiHttpClient();
			return new TranslationService(http, sp.GetRequiredService<IAiSettingsService>());
		});
		builder.Services.AddSingleton<AppShell>();

		builder.Services.AddTransient<HomeViewModel>();
		builder.Services.AddTransient<HandbookViewModel>();
		builder.Services.AddTransient<HandbookCategoryViewModel>();
		builder.Services.AddTransient<ArticleViewModel>();
		builder.Services.AddTransient<FirstStepsViewModel>();
		builder.Services.AddTransient<ChecklistViewModel>();
		builder.Services.AddTransient<NewsViewModel>();
		builder.Services.AddTransient<ChatViewModel>();

		return builder.Build();
	}

	private static HttpClient CreateAiHttpClient()
	{
		var http = new HttpClient { Timeout = TimeSpan.FromSeconds(300) };
		http.DefaultRequestHeaders.UserAgent.ParseAdd("HimiMaui/1.0 (+ai-client)");
		return http;
	}
}
