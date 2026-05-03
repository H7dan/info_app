using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Himi.Services;

namespace Himi.ViewModels;

public sealed class HandbookCategoryItemVm
{
	public required string Id { get; init; }
	public required string Title { get; init; }
	public required int Count { get; init; }
	public string CountLabel => Count.ToString();
}

public sealed class HandbookViewModel : INotifyPropertyChanged
{
	private readonly IContentRepository _contentRepository;
	private readonly ILanguageService _languageService;

	private bool _isLoading;

	public event PropertyChangedEventHandler? PropertyChanged;

	public ObservableCollection<HandbookCategoryItemVm> Categories { get; } = new();

	public bool IsLoading
	{
		get => _isLoading;
		private set
		{
			if (_isLoading == value) return;
			_isLoading = value;
			OnPropertyChanged();
		}
	}

	public HandbookViewModel(IContentRepository contentRepository, ILanguageService languageService)
	{
		_contentRepository = contentRepository;
		_languageService = languageService;

		_languageService.PropertyChanged += (_, e) =>
		{
			if (e.PropertyName == nameof(ILanguageService.CurrentLanguage))
				_ = LoadAsync();
		};
	}

	public async Task LoadAsync()
	{
		if (IsLoading) return;
		IsLoading = true;
		try
		{
			var index = await _contentRepository.GetIndexAsync();
			var lang = _languageService.CurrentLanguage;

			var articleCountsByCategory = index.Articles
				.Where(a => a.Lang == lang)
				.GroupBy(a => a.CategoryId)
				.ToDictionary(g => g.Key, g => g.Count());

			var items = index.Categories
				.OrderBy(c => c.Sort)
				.Select(c => new HandbookCategoryItemVm
				{
					Id = c.Id,
					Title = LocalizeCategoryTitle(lang, c.Id, c.Title),
					Count = articleCountsByCategory.TryGetValue(c.Id, out var count) ? count : 0
				})
				.ToList();

			Categories.Clear();
			foreach (var i in items)
				Categories.Add(i);
		}
		finally
		{
			IsLoading = false;
		}
	}

	private static string LocalizeCategoryTitle(string lang, string id, string fallback) => (lang, id) switch
	{
		("ru", "EmergencyNumbers") => "Экстренные номера",
		("ru", "EmergencyAndHealth") => "Экстренное и здоровье",
		("ru", "LegalizationAndDocuments") => "Документы и легализация",
		("ru", "DoNotBreakRules") => "Не нарушай правила",
		("ru", "DailyLife") => "Быт",
		("ru", "Transport") => "Транспорт",
		("ru", "Driving") => "Вождение",
		("ru", "Housing") => "Жильё",
		("ru", "WorkAndMoney") => "Работа и деньги",
		("ru", "KidsAndEducation") => "Дети и образование",

		("ua", "EmergencyNumbers") => "Екстрені номери",
		("ua", "EmergencyAndHealth") => "Екстрене та здоров'я",
		("ua", "LegalizationAndDocuments") => "Документи та легалізація",
		("ua", "DoNotBreakRules") => "Не порушуй правила",
		("ua", "DailyLife") => "Повсякденне життя",
		("ua", "Transport") => "Транспорт",
		("ua", "Driving") => "Водіння",
		("ua", "Housing") => "Житло",
		("ua", "WorkAndMoney") => "Робота і гроші",
		("ua", "KidsAndEducation") => "Діти й освіта",

		("pl", "EmergencyNumbers") => "Numery alarmowe",
		("pl", "EmergencyAndHealth") => "Nagłe przypadki i zdrowie",
		("pl", "LegalizationAndDocuments") => "Dokumenty i legalizacja",
		("pl", "DoNotBreakRules") => "Nie łam zasad",
		("pl", "DailyLife") => "Codzienne życie",
		("pl", "Transport") => "Transport",
		("pl", "Driving") => "Jazda",
		("pl", "Housing") => "Mieszkanie",
		("pl", "WorkAndMoney") => "Praca i pieniądze",
		("pl", "KidsAndEducation") => "Dzieci i edukacja",

		_ => fallback
	};

	private void OnPropertyChanged([CallerMemberName] string? name = null)
		=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

