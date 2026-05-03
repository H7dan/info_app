using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Himi.Models;
using Himi.Services;

namespace Himi.ViewModels;

public sealed class ChecklistItemVm : INotifyPropertyChanged
{
	private bool _isDone;

	public event PropertyChangedEventHandler? PropertyChanged;

	public required string Id { get; init; }
	public required string Text { get; init; }
	public required string? LinksToArticleId { get; init; }

	public bool IsDone
	{
		get => _isDone;
		set
		{
			if (_isDone == value) return;
			_isDone = value;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDone)));
		}
	}
}

public sealed class ChecklistSectionVm
{
	public required string Id { get; init; }
	public required string Title { get; init; }
	public required ObservableCollection<ChecklistItemVm> Items { get; init; }
}

public sealed class ChecklistViewModel : INotifyPropertyChanged
{
	private readonly IContentRepository _contentRepository;
	private readonly ILanguageService _languageService;
	private readonly IChecklistProgressStore _progress;

	private string _title = "Checklist";
	private bool _isLoading;
	private string? _checklistId;
	private Dictionary<string, Article> _articlesById = new();
	private HashSet<string> _completed = new();

	public event PropertyChangedEventHandler? PropertyChanged;

	public ObservableCollection<ChecklistSectionVm> Sections { get; } = new();

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

	public ChecklistViewModel(
		IContentRepository contentRepository,
		ILanguageService languageService,
		IChecklistProgressStore progress)
	{
		_contentRepository = contentRepository;
		_languageService = languageService;
		_progress = progress;

		_languageService.PropertyChanged += (_, e) =>
		{
			if (e.PropertyName == nameof(ILanguageService.CurrentLanguage))
				_ = LoadAsync(_checklistId);
		};
	}

	public async Task LoadAsync(string? checklistId)
	{
		if (string.IsNullOrWhiteSpace(checklistId))
			return;

		if (IsLoading) return;
		IsLoading = true;
		try
		{
			_checklistId = checklistId;
			var index = await _contentRepository.GetIndexAsync();
			var lang = _languageService.CurrentLanguage;

			_articlesById = index.Articles
				.Where(a => a.Lang == lang)
				.GroupBy(a => a.Id)
				.ToDictionary(g => g.Key, g => g.First());

			var checklist = index.Checklists.FirstOrDefault(c => c.Lang == lang && c.Id == checklistId);
			if (checklist is null)
			{
				Title = "Checklist";
				Sections.Clear();
				return;
			}

			Title = checklist.Title;
			_completed = await _progress.GetCompletedAsync(lang, checklistId);

			var newSections = checklist.Sections.Select(s =>
			{
				var items = new ObservableCollection<ChecklistItemVm>(
					s.Items.Select(i =>
					{
						var vm = new ChecklistItemVm
						{
							Id = i.Id,
							Text = i.Text,
							LinksToArticleId = i.LinksToArticleId,
							IsDone = _completed.Contains(i.Id)
						};

						vm.PropertyChanged += (_, e) =>
						{
							if (e.PropertyName == nameof(ChecklistItemVm.IsDone))
								_ = SaveProgressAsync();
						};

						return vm;
					}));

				return new ChecklistSectionVm
				{
					Id = s.Id,
					Title = s.Title,
					Items = items
				};
			}).ToList();

			Sections.Clear();
			foreach (var s in newSections)
				Sections.Add(s);
		}
		finally
		{
			IsLoading = false;
		}
	}

	public bool TryGetArticle(string articleId, out Article article)
		=> _articlesById.TryGetValue(articleId, out article!);

	private async Task SaveProgressAsync()
	{
		var id = _checklistId;
		if (string.IsNullOrWhiteSpace(id))
			return;

		var lang = _languageService.CurrentLanguage;
		var completed = Sections
			.SelectMany(s => s.Items)
			.Where(i => i.IsDone)
			.Select(i => i.Id)
			.ToHashSet();

		_completed = completed;
		await _progress.SetCompletedAsync(lang, id, completed);
	}

	private void OnPropertyChanged([CallerMemberName] string? name = null)
		=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

