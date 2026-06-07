using System.Text.Json;
using Himi.Models;

namespace Himi.Services;

public interface IChatHistoryStore
{
	Task<IReadOnlyList<ChatMessage>> LoadAsync(CancellationToken cancellationToken = default);
	Task SaveAsync(IReadOnlyList<ChatMessage> messages, CancellationToken cancellationToken = default);
	Task ClearAsync(CancellationToken cancellationToken = default);
}

public sealed class ChatHistoryStore : IChatHistoryStore
{
	private const int SchemaVersion = 1;
	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		PropertyNameCaseInsensitive = true,
		WriteIndented = true
	};

	private static string HistoryPath => Path.Combine(FileSystem.AppDataDirectory, "chat_history.json");

	public async Task<IReadOnlyList<ChatMessage>> LoadAsync(CancellationToken cancellationToken = default)
	{
		if (!File.Exists(HistoryPath))
			return Array.Empty<ChatMessage>();

		var json = await File.ReadAllTextAsync(HistoryPath, cancellationToken);
		var history = JsonSerializer.Deserialize<ChatHistory>(json, JsonOptions);
		return history?.Messages ?? Array.Empty<ChatMessage>();
	}

	public async Task SaveAsync(IReadOnlyList<ChatMessage> messages, CancellationToken cancellationToken = default)
	{
		Directory.CreateDirectory(FileSystem.AppDataDirectory);
		var history = new ChatHistory(SchemaVersion, messages);
		var json = JsonSerializer.Serialize(history, JsonOptions);
		await File.WriteAllTextAsync(HistoryPath, json, cancellationToken);
	}

	public Task ClearAsync(CancellationToken cancellationToken = default)
	{
		if (File.Exists(HistoryPath))
			File.Delete(HistoryPath);
		return Task.CompletedTask;
	}
}
