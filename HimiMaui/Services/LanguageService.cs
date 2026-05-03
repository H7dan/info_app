using System.ComponentModel;
using System.Globalization;

namespace Himi.Services;

public interface ILanguageService : INotifyPropertyChanged
{
	string CurrentLanguage { get; }
	IReadOnlyList<string> SupportedLanguages { get; }
	void SetLanguage(string languageCode);
	void CycleNextLanguage();
}

public sealed class LanguageService : ILanguageService
{
	private const string PreferenceKey = "language_code";
	private static readonly IReadOnlyList<string> Supported = new[] { "ua", "pl", "ru" };

	public event PropertyChangedEventHandler? PropertyChanged;

	public IReadOnlyList<string> SupportedLanguages => Supported;

	public string CurrentLanguage { get; private set; } = "ua";

	public LanguageService()
	{
		var saved = Preferences.Get(PreferenceKey, "ua");
		// Back-compat for older builds that stored Ukrainian as "uk".
		if (saved == "uk") saved = "ua";
		SetLanguage(IsSupported(saved) ? saved : "ua");
	}

	public void SetLanguage(string languageCode)
	{
		// Normalize to keep a single code in the app state.
		if (languageCode == "uk") languageCode = "ua";

		if (!IsSupported(languageCode))
			return;

		if (CurrentLanguage == languageCode)
			return;

		CurrentLanguage = languageCode;
		Preferences.Set(PreferenceKey, languageCode);

		// We keep this simple for MVP: update global cultures and let pages refresh themselves.
		// "ua" is not a valid BCP-47 language tag; map to Ukrainian culture.
		var culture = new CultureInfo(languageCode == "ua" ? "uk-UA" : languageCode);
		CultureInfo.DefaultThreadCurrentCulture = culture;
		CultureInfo.DefaultThreadCurrentUICulture = culture;

		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentLanguage)));
	}

	public void CycleNextLanguage()
	{
		var idx = -1;
		for (var i = 0; i < Supported.Count; i++)
		{
			if (Supported[i] == CurrentLanguage)
			{
				idx = i;
				break;
			}
		}

		var next = idx < 0 ? Supported[0] : Supported[(idx + 1) % Supported.Count];
		SetLanguage(next);
	}

	private static bool IsSupported(string code) => Supported.Contains(code);
}

