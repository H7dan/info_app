using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Himi.Models;
using Himi.Services;

namespace Himi.ViewModels;

public sealed class ChecklistListItemVm
{
	public required string Id { get; init; }
	public required string Title { get; init; }
	public required int TotalItems { get; init; }
	public string Meta => TotalItems == 0 ? string.Empty : $"{TotalItems}";
}

public sealed class FirstStepsViewModel : INotifyPropertyChanged
{
	private readonly IContentRepository _contentRepository;
	private readonly ILanguageService _languageService;

	private bool _isLoading;

	public event PropertyChangedEventHandler? PropertyChanged;

	public ObservableCollection<ChecklistListItemVm> Checklists { get; } = new();

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

	public FirstStepsViewModel(IContentRepository contentRepository, ILanguageService languageService)
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

			var items = index.Checklists
				.Where(c => c.Lang == lang)
				.Select(c => new ChecklistListItemVm
				{
					Id = c.Id,
					Title = c.Title,
					TotalItems = c.Sections.Sum(s => s.Items.Count)
				})
				// Default order for MVP.
				.OrderBy(c => c.Id switch { "day1" => 10, "week1" => 20, "month1" => 30, _ => 100 })
				.ThenBy(c => c.Title)
				.ToList();

			Checklists.Clear();
			foreach (var i in items)
				Checklists.Add(i);
		}
		finally
		{
			IsLoading = false;
		}
	}

	private void OnPropertyChanged([CallerMemberName] string? name = null)
		=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

