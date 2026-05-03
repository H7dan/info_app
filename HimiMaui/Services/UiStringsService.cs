using System.ComponentModel;

namespace Himi.Services;

public interface IUiStringsService : INotifyPropertyChanged
{
	string HomeDescription { get; }
	string HomeFirstSteps { get; }
	string HomeHandbook { get; }
	string HomeNews { get; }

	string PhonesTitle { get; }
	string PhonesEmptyMessage { get; }
	string Cancel { get; }
	string Ok { get; }

	string Error { get; }
}

public sealed class UiStringsService : IUiStringsService
{
	private readonly ILanguageService _language;

	public event PropertyChangedEventHandler? PropertyChanged;

	public UiStringsService(ILanguageService language)
	{
		_language = language;
		_language.PropertyChanged += (_, e) =>
		{
			if (e.PropertyName == nameof(ILanguageService.CurrentLanguage))
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
		};
	}

	public string HomeDescription => L(
		"Довідник допомоги українцям, які іммігрували до Польщі.",
		"Przewodnik pomocy dla Ukraińców, którzy imigrowali do Polski.",
		"Справочник помощи украинцам, которые иммигрировали в Польшу."
	);

	public string HomeFirstSteps => L("Перші кроки", "Pierwsze kroki", "Первые шаги");
	public string HomeHandbook => L("Довідник", "Baza wiedzy", "Справочник");
	public string HomeNews => L("Новини", "Aktualności", "Новости");

	public string PhonesTitle => L("Екстрені номери", "Numery alarmowe", "Экстренные номера");
	public string PhonesEmptyMessage => L(
		"Немає екстрених номерів для цієї мови.",
		"Brak numerów alarmowych dla tego języka.",
		"Нет экстренных номеров для этого языка."
	);

	public string Cancel => L("Скасувати", "Anuluj", "Отмена");
	public string Ok => "OK";

	public string Error => L("Помилка", "Błąd", "Ошибка");

	private string L(string uk, string pl, string ru) => _language.CurrentLanguage switch
	{
		"pl" => pl,
		"ru" => ru,
		_ => uk
	};
}

