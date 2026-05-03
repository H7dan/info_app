using System.ComponentModel;

namespace Himi.Services;

public interface IThemeService : INotifyPropertyChanged
{
	AppTheme UserTheme { get; }
	void ToggleTheme();
}

public sealed class ThemeService : IThemeService
{
	private const string PreferenceKey = "user_theme";

	public event PropertyChangedEventHandler? PropertyChanged;

	public AppTheme UserTheme { get; private set; } = AppTheme.Unspecified;

	public ThemeService()
	{
		var saved = Preferences.Get(PreferenceKey, "system");
		UserTheme = saved switch
		{
			"light" => AppTheme.Light,
			"dark" => AppTheme.Dark,
			_ => AppTheme.Unspecified
		};

		Apply(UserTheme);
	}

	public void ToggleTheme()
	{
		var current = Application.Current?.UserAppTheme ?? AppTheme.Unspecified;
		var next = current == AppTheme.Dark ? AppTheme.Light : AppTheme.Dark;
		Apply(next);
	}

	private void Apply(AppTheme theme)
	{
		UserTheme = theme;

		if (Application.Current is not null)
			Application.Current.UserAppTheme = theme;

		var persisted = theme switch
		{
			AppTheme.Light => "light",
			AppTheme.Dark => "dark",
			_ => "system"
		};
		Preferences.Set(PreferenceKey, persisted);

		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UserTheme)));
	}
}

