using System.Text.Json;

namespace Himi.Services;

public interface IChecklistProgressStore
{
	Task<HashSet<string>> GetCompletedAsync(string language, string checklistId, CancellationToken cancellationToken = default);
	Task SetCompletedAsync(string language, string checklistId, HashSet<string> completedItemIds, CancellationToken cancellationToken = default);
}

public sealed class ChecklistProgressStore : IChecklistProgressStore
{
	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		PropertyNameCaseInsensitive = true,
		WriteIndented = false
	};

	private static string Key(string language, string checklistId) => $"checklist_progress::{language}::{checklistId}";

	public Task<HashSet<string>> GetCompletedAsync(string language, string checklistId, CancellationToken cancellationToken = default)
	{
		var json = Preferences.Get(Key(language, checklistId), string.Empty);
		if (string.IsNullOrWhiteSpace(json))
			return Task.FromResult(new HashSet<string>());

		try
		{
			var parsed = JsonSerializer.Deserialize<HashSet<string>>(json, JsonOptions);
			return Task.FromResult(parsed ?? new HashSet<string>());
		}
		catch
		{
			// If the payload gets corrupted, keep the app usable and let user re-check items.
			return Task.FromResult(new HashSet<string>());
		}
	}

	public Task SetCompletedAsync(string language, string checklistId, HashSet<string> completedItemIds, CancellationToken cancellationToken = default)
	{
		var json = JsonSerializer.Serialize(completedItemIds, JsonOptions);
		Preferences.Set(Key(language, checklistId), json);
		return Task.CompletedTask;
	}
}

