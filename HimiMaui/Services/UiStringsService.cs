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
	string NoInternet { get; }

	string TranslateArticle { get; }
	string AskAboutArticle { get; }
	string ViewOriginal { get; }
	string ViewTranslation { get; }

	string ChatTitle { get; }
	string ChatInputPlaceholder { get; }
	string ChatSend { get; }
	string ChatDisclaimer { get; }
	string AiStubMode { get; }

	string NewsPageTitle { get; }
	string NewsSearchPlaceholder { get; }
	string ConnectionFailed { get; }
	string ServerError { get; }
	string ArticleContextMissing { get; }
	string HandbookSearchPlaceholder { get; }
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

	public string NoInternet => L(
		"Немає інтернету. Увімкніть stub-режим або підключення.",
		"Brak internetu. Włącz tryb stub lub połączenie.",
		"Нет интернета. Включите stub-режим или подключение."
	);

	public string TranslateArticle => L("Перекласти", "Przetłumacz", "Перевести");
	public string AskAboutArticle => L("Запитати", "Zapytaj", "Спросить");
	public string ViewOriginal => L("Оригінал", "Oryginał", "Оригинал");
	public string ViewTranslation => L("Переклад", "Tłumaczenie", "Перевод");

	public string ChatTitle => L("Чат", "Czat", "Чат");
	public string ChatInputPlaceholder => L("Повідомлення…", "Wiadomość…", "Сообщение…");
	public string ChatSend => L("Надіслати", "Wyślij", "Отправить");
	public string ChatDisclaimer => L(
		"AI не замінює юридичну консультацію.",
		"AI nie zastępuje porady prawnej.",
		"AI не заменяет юридическую консультацию."
	);

	public string AiStubMode => L("Заглушка AI", "AI (test)", "Заглушка AI");

	public string NewsPageTitle => HomeNews;

	public string NewsSearchPlaceholder => L(
		"Пошук за заголовком",
		"Szukaj po tytule",
		"Поиск по заголовку"
	);

	public string HandbookSearchPlaceholder => L("Пошук", "Szukaj", "Поиск");

	public string ConnectionFailed => L(
		"Немає з'єднання з AI-сервером. Перевірте Docker і що stub вимкнено.",
		"Brak połączenia z serwerem AI. Sprawdź Docker i wyłącz tryb testowy.",
		"Нет соединения с AI-сервером. Проверьте Docker и что заглушка выключена."
	);

	public string ServerError => L(
		"Помилка AI-сервера. Спробуйте ще раз або перевірте логи Docker.",
		"Błąd serwera AI. Spróbuj ponownie lub sprawdź logi Dockera.",
		"Ошибка AI-сервера. Попробуйте снова или проверьте логи Docker."
	);

	public string ArticleContextMissing => L(
		"Текст статті не завантажено. Відкрийте новину знову.",
		"Nie udało się wczytać treści artykułu. Otwórz wiadomość ponownie.",
		"Текст статьи не загружен. Откройте новость снова."
	);

	private string L(string uk, string pl, string ru) => _language.CurrentLanguage switch
	{
		"pl" => pl,
		"ru" => ru,
		_ => uk
	};
}

