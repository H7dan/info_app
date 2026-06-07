using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Himi.Models;
using Himi.Services;

namespace Himi.ViewModels;

public sealed class ArticleListItemVm
{
	public required string Id { get; init; }
	public required string Title { get; init; }
	public required string Summary { get; init; }
	public required string BodyPath { get; init; }
	public required string CategoryId { get; init; }
}

public sealed class HandbookCategoryViewModel : INotifyPropertyChanged
{
	private readonly IContentRepository _contentRepository;
	private readonly ILanguageService _languageService;
	private readonly IUiStringsService _strings;

	private string _title = "Handbook";
	private bool _isLoading;
	private string _searchText = string.Empty;
	private string? _categoryId;
	private List<Article> _all = new();

	public event PropertyChangedEventHandler? PropertyChanged;

	public ObservableCollection<ArticleListItemVm> Articles { get; } = new();

	public string Title
	{
		get => _title;
		private set
		{
			if (_title == value) return;
			_title = value;
			OnPropertyChanged();
		}
	}

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

	public string SearchText
	{
		get => _searchText;
		set
		{
			if (_searchText == value) return;
			_searchText = value ?? string.Empty;
			OnPropertyChanged();
			ApplyFilter();
		}
	}

	public string SearchPlaceholder => _strings.HandbookSearchPlaceholder;

	public HandbookCategoryViewModel(
		IContentRepository contentRepository,
		ILanguageService languageService,
		IUiStringsService strings)
	{
		_contentRepository = contentRepository;
		_languageService = languageService;
		_strings = strings;

		_languageService.PropertyChanged += (_, e) =>
		{
			if (e.PropertyName == nameof(ILanguageService.CurrentLanguage))
				_ = LoadAsync(_categoryId);
		};

		_strings.PropertyChanged += (_, _) => OnPropertyChanged(nameof(SearchPlaceholder));
	}

	public async Task LoadAsync(string? categoryId)
	{
		if (string.IsNullOrWhiteSpace(categoryId))
			return;

		if (IsLoading) return;
		IsLoading = true;
		try
		{
			_categoryId = categoryId;
			var index = await _contentRepository.GetIndexAsync();
			var lang = _languageService.CurrentLanguage;

			var category = index.Categories.FirstOrDefault(c => c.Id == categoryId);
			Title = category is null ? "Handbook" : LocalizeCategoryTitle(lang, category.Id, category.Title);

			_all = index.Articles
				.Where(a => a.Lang == lang && a.CategoryId == categoryId)
				.OrderBy(a => a.Title)
				.ToList();

			ApplyFilter();
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

	private void ApplyFilter()
	{
		var q = (SearchText ?? string.Empty).Trim();

		var filtered = string.IsNullOrWhiteSpace(q)
			? _all
			: _all.Where(a =>
					a.Title.Contains(q, StringComparison.CurrentCultureIgnoreCase)
					|| a.Summary.Contains(q, StringComparison.CurrentCultureIgnoreCase)
					|| a.Tags.Any(t => t.Contains(q, StringComparison.CurrentCultureIgnoreCase)))
				.ToList();

		Articles.Clear();
		foreach (var a in filtered)
		{
			Articles.Add(new ArticleListItemVm
			{
				Id = a.Id,
				Title = a.Title,
				Summary = a.Summary,
				BodyPath = a.BodyPath,
				CategoryId = a.CategoryId
			});
		}
	}

	private void OnPropertyChanged([CallerMemberName] string? name = null)
		=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

