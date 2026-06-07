using System.ComponentModel;

namespace Himi.Services;

public interface IAiSettingsService : INotifyPropertyChanged
{
	string BaseUrl { get; }
	bool StubMode { get; }
	void SetStubMode(bool enabled);
}

public sealed class AiSettingsService : IAiSettingsService
{
	private const string StubModeKey = "ai_stub_mode";

	public event PropertyChangedEventHandler? PropertyChanged;

	public string BaseUrl =>
#if ANDROID
		"http://10.0.2.2:8000";
#else
		"http://localhost:8000";
#endif

	public bool StubMode => Preferences.Get(StubModeKey, false);

	public void SetStubMode(bool enabled)
	{
		Preferences.Set(StubModeKey, enabled);
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StubMode)));
	}
}
