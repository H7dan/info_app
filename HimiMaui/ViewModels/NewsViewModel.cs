using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Himi.Models;
using Himi.Services;

namespace Himi.ViewModels;

public sealed class NewsListItemVm
{
	public required string Id { get; init; }
	public required string Title { get; init; }
	public required string BodyPath { get; init; }
	public required DateTime PublishedUtc { get; init; }
	public string PublishedLabel => PublishedUtc.ToLocalTime().ToString("yyyy-MM-dd");
	public string? Intro { get; init; }
	public required string Url { get; init; }
}

public sealed class NewsViewModel : INotifyPropertyChanged
{
	private readonly INewsService _news;

	private bool _isLoading;
	private string _query = string.Empty;
	private List<NewsItem> _all = new();

	public event PropertyChangedEventHandler? PropertyChanged;

	public ObservableCollection<NewsListItemVm> Items { get; } = new();

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

	public string Query
	{
		get => _query;
		set
		{
			if (_query == value) return;
			_query = value;
			OnPropertyChanged();
			ApplyFilter();
		}
	}

	public ICommand RefreshCommand { get; }

	public NewsViewModel(INewsService news)
	{
		_news = news;
		RefreshCommand = new Command(async () => await RefreshAsync());
	}

	public async Task LoadCachedAsync()
	{
		_all = (await _news.GetCachedAsync()).ToList();
		ApplyFilter();
	}

	public async Task RefreshAsync()
	{
		if (IsLoading) return;
		IsLoading = true;
		try
		{
			_all = (await _news.RefreshAsync()).ToList();
			ApplyFilter();
		}
		finally
		{
			IsLoading = false;
		}
	}

	private void ApplyFilter()
	{
		var q = (Query ?? string.Empty).Trim();
		var filtered = string.IsNullOrWhiteSpace(q)
			? _all
			: _all.Where(i => i.Title.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();

		Items.Clear();
		foreach (var i in filtered.OrderByDescending(x => x.PublishedUtc))
		{
			Items.Add(new NewsListItemVm
			{
				Id = i.Id,
				Title = i.Title,
				BodyPath = i.BodyPath,
				PublishedUtc = i.PublishedUtc,
				Intro = i.Intro,
				Url = i.Url
			});
		}
	}

	private void OnPropertyChanged([CallerMemberName] string? name = null)
		=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

