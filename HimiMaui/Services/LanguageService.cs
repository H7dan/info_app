using System.ComponentModel;

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

		// UI strings are resolved via IUiStringsService; do not set thread culture here —
		// on Android it can force the soft keyboard language and block typing in other languages.

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

