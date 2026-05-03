using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Himi.Models;
using Himi.Services;

namespace Himi.ViewModels;

public sealed class HomeViewModel : INotifyPropertyChanged
{
	private readonly IContentRepository _contentRepository;
	private readonly ILanguageService _languageService;
	private readonly IThemeService _themeService;
	private readonly IUiStringsService _strings;

	private bool _isLoading;

	public event PropertyChangedEventHandler? PropertyChanged;

	public ObservableCollection<PhoneContact> EmergencyContacts { get; } = new();

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

	public string CurrentLanguage => _languageService.CurrentLanguage;
	public string CurrentLanguageLabel => _languageService.CurrentLanguage switch
	{
		"ua" => "UA",
		"ru" => "RU",
		"pl" => "PL",
		_ => _languageService.CurrentLanguage.ToUpperInvariant()
	};

	public ICommand CycleLanguageCommand { get; }
	public ICommand ToggleThemeCommand { get; }

	public string HomeDescription => _strings.HomeDescription;
	public string HomeFirstSteps => _strings.HomeFirstSteps;
	public string HomeHandbook => _strings.HomeHandbook;
	public string HomeNews => _strings.HomeNews;

	public string PhonesTitle => _strings.PhonesTitle;
	public string PhonesEmptyMessage => _strings.PhonesEmptyMessage;
	public string Cancel => _strings.Cancel;
	public string Ok => _strings.Ok;
	public string Error => _strings.Error;

	public HomeViewModel(
		IContentRepository contentRepository,
		ILanguageService languageService,
		IThemeService themeService,
		IUiStringsService strings)
	{
		_contentRepository = contentRepository;
		_languageService = languageService;
		_themeService = themeService;
		_strings = strings;

		CycleLanguageCommand = new Command(() =>
		{
			_languageService.CycleNextLanguage();
			OnPropertyChanged(nameof(CurrentLanguage));
			OnPropertyChanged(nameof(CurrentLanguageLabel));
			_ = LoadAsync();
		});

		ToggleThemeCommand = new Command(() => _themeService.ToggleTheme());

		_languageService.PropertyChanged += (_, e) =>
		{
			if (e.PropertyName == nameof(ILanguageService.CurrentLanguage))
			{
				OnPropertyChanged(nameof(CurrentLanguage));
				OnPropertyChanged(nameof(CurrentLanguageLabel));
				OnPropertyChanged(nameof(HomeDescription));
				OnPropertyChanged(nameof(HomeFirstSteps));
				OnPropertyChanged(nameof(HomeHandbook));
				OnPropertyChanged(nameof(HomeNews));
				OnPropertyChanged(nameof(PhonesTitle));
				OnPropertyChanged(nameof(PhonesEmptyMessage));
				OnPropertyChanged(nameof(Cancel));
				OnPropertyChanged(nameof(Ok));
				OnPropertyChanged(nameof(Error));
			}
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

			var contacts = index.Contacts
				.Where(c => c.Lang == lang && c.Category == "Emergency")
				.OrderBy(c => c.Number)
				.ToList();

			EmergencyContacts.Clear();
			foreach (var c in contacts)
				EmergencyContacts.Add(c);
		}
		finally
		{
			IsLoading = false;
		}
	}

	private void OnPropertyChanged([CallerMemberName] string? name = null)
		=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

